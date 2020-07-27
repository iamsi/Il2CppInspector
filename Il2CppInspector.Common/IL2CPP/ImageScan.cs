﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using NoisyCowStudios.Bin2Object;

namespace Il2CppInspector
{
    partial class Il2CppBinary
    {
        // Find a sequence of bytes
        private int FindBytes(byte[] blob, byte[] signature, int step = 1, int startOffset = 0) {
            int sigMatch = 0;
            int foundOffset = -1;

            for (int p = startOffset; p < blob.Length - signature.Length && foundOffset == -1; p += sigMatch == 0? step : 1) {
                sigMatch = blob[p] != signature[sigMatch] ? 0 : ++sigMatch;

                if (sigMatch == 0)
                    p = p - p % step + step;

                if (sigMatch == 0 && blob[p] == signature[0])
                    sigMatch++;

                if (sigMatch == signature.Length)
                    foundOffset = p + 1 - signature.Length;
            }
            return foundOffset;
        }

        // Find all occurrences of a sequence of bytes
        private IEnumerable<uint> FindAllBytes(byte[] blob, byte[] signature, int step = 0) {
            var ptrs = new List<uint>();
            var offset = 0;
            while (offset != -1) {
                offset = FindBytes(blob, signature, step != 0 ? step : Image.Bits / 4, offset);
                if (offset != -1) {
                    yield return (uint) offset;
                    offset += Image.Bits / 4;
                }
            }
        }

        // Find strings
        private IEnumerable<uint> FindAllStrings(byte[] blob, string str) => FindAllBytes(blob, Encoding.ASCII.GetBytes(str), 1);

        // Find 32-bit words
        private IEnumerable<uint> FindAllDWords(byte[] blob, uint word) => FindAllBytes(blob, BitConverter.GetBytes(word), 4);

        // Find 64-bit words
        private IEnumerable<uint> FindAllQWords(byte[] blob, ulong word) => FindAllBytes(blob, BitConverter.GetBytes(word), 8);

        // Find words for the current binary size
        private IEnumerable<uint> FindAllWords(byte[] blob, ulong word)
            => Image.Bits switch {
                32 => FindAllDWords(blob, (uint) word),
                64 => FindAllQWords(blob, word),
                _ => throw new InvalidOperationException("Invalid architecture bit size")
            };

        // Find all valid virtual address pointers to a virtual address
        private IEnumerable<ulong> FindAllMappedWords(byte[] blob, ulong va) {
            var fileOffsets = FindAllWords(blob, va);
            foreach (var offset in fileOffsets)
                if (Image.TryMapFileOffsetToVA(offset, out va))
                    yield return va;
        }

        // Find all valid virtual address pointers to a set of virtual addresses
        private IEnumerable<ulong> FindAllMappedWords(byte[] blob, IEnumerable<ulong> va) => va.SelectMany(a => FindAllMappedWords(blob, a));

        // Find all valid pointer chains to a set of virtual addresses with the specified number of indirections
        private IEnumerable<ulong> FindAllPointerChains(byte[] blob, IEnumerable<ulong> va, int indirections) {
            IEnumerable<ulong> vas = va;
            for (int i = 0; i < indirections; i++)
                vas = FindAllMappedWords(blob, vas);
            return vas;
        }

        // Scan the image for the needed data structures
        private (ulong, ulong) ImageScan(Metadata metadata) {
            Image.Position = 0;
            var imageBytes = Image.ReadBytes((int) Image.Length);

            var ptrSize = (uint) Image.Bits / 8;
            ulong codeRegistration = 0;
            IEnumerable<ulong> vas;

            // Find CodeRegistration
            // >= 24.2
            if (metadata.Version >= 24.2) {
                var offsets = FindAllStrings(imageBytes, "mscorlib.dll\0");
                vas = offsets.Select(o => Image.MapFileOffsetToVA(o));

                // Unwind from string pointer -> CodeGenModule -> CodeGenModules -> CodeRegistration + x
                vas = FindAllPointerChains(imageBytes, vas, 3);

                if (!vas.Any())
                    return (0, 0);

                if (vas.Count() > 1)
                    throw new InvalidOperationException("More than one valid pointer chain found during data heuristics");

                // pCodeGenModules is the last field in CodeRegistration so we subtract the size of one pointer from the struct size
                codeRegistration = vas.First() - ((ulong) Metadata.Sizeof(typeof(Il2CppCodeRegistration), Image.Version, Image.Bits / 8) - ptrSize);

                // In v24.3, windowsRuntimeFactoryTable collides with codeGenModules. So far no samples have had windowsRuntimeFactoryCount > 0;
                // if this changes we'll have to get smarter about disambiguating these two.
                var cr = Image.ReadMappedObject<Il2CppCodeRegistration>(codeRegistration);

                if (Image.Version == 24.2 && cr.interopDataCount == 0) {
                    Image.Version = 24.3;
                    codeRegistration -= ptrSize * 2; // two extra words for WindowsRuntimeFactory
                }
            }

            // Find CodeRegistration
            // <= 24.1
            else {
                // The first item in CodeRegistration is the total number of method pointers
                vas = FindAllMappedWords(imageBytes, (ulong) metadata.Methods.Count(m => (uint) m.methodIndex != 0xffff_ffff));

                if (!vas.Any())
                    return (0, 0);

                // The count of method pointers will be followed some bytes later by
                // the count of custom attribute generators; the distance between them
                // depends on the il2cpp version so we just use ReadMappedObject to simplify the math
                foreach (var va in vas) {
                    var cr = Image.ReadMappedObject<Il2CppCodeRegistration>(va);

                    if (cr.customAttributeCount == metadata.AttributeTypeRanges.Length)
                        codeRegistration = va;
                }

                if (codeRegistration == 0)
                    return (0, 0);
            }

            // Find MetadataRegistration
            // >= 19
            var metadataRegistration = 0ul;

            // Find TypeDefinitionsSizesCount (4th last field) then work back to the start of the struct
            // This saves us from guessing where metadataUsagesCount is later
            var mrSize = (ulong) Metadata.Sizeof(typeof(Il2CppMetadataRegistration), Image.Version, Image.Bits / 8);
            vas = FindAllMappedWords(imageBytes, (ulong) metadata.Types.Length).Select(a => a - mrSize + ptrSize * 4);

            foreach (var va in vas) {
                var mr = Image.ReadMappedObject<Il2CppMetadataRegistration>(va);
                if (mr.metadataUsagesCount == (ulong) metadata.MetadataUsageLists.Length)
                    metadataRegistration = va;
            }

            if (metadataRegistration == 0)
                return (0, 0);

            return (codeRegistration, metadataRegistration);
        }
    }
}
