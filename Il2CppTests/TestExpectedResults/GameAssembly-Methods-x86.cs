﻿// Image 0: mscorlib.dll - 0
// Image 1: Methods.dll - 1810

// Namespace: Il2CppTests.TestSources
internal class Test // TypeDefIndex: 1811
{
	// Constructors
	public Test(); // 0x100BF000

	// Methods
	public virtual void VirtualMethod(); // 0x100C5530
	public int ValueTypeReturnMethod(); // 0x100C5600
	public StringBuilder ReferenceTypeReturnMethod(); // 0x1034DA60
}

// Namespace: Il2CppTests.TestSources
internal abstract class TestAbstract // TypeDefIndex: 1812
{
	// Constructors
	protected TestAbstract(); // 0x100BF000

	// Methods
	public abstract void AbstractMethod();
}

// Namespace: Il2CppTests.TestSources
internal class TestOverride : Test // TypeDefIndex: 1813
{
	// Constructors
	public TestOverride(); // 0x100BF000

	// Methods
	public override void VirtualMethod(); // 0x100C5530
}

// Namespace: Il2CppTests.TestSources
internal class TestHideVirtual : Test // TypeDefIndex: 1814
{
	// Constructors
	public TestHideVirtual(); // 0x100BF000

	// Methods
	public new void VirtualMethod(); // 0x100C5530
}

// Namespace: Il2CppTests.TestSources
internal class TestHideOverride : TestOverride // TypeDefIndex: 1815
{
	// Constructors
	public TestHideOverride(); // 0x100BF000

	// Methods
	public new void VirtualMethod(); // 0x100C5530
}

// Namespace: Il2CppTests.TestSources
internal class TestOverrideAbstract : TestAbstract // TypeDefIndex: 1816
{
	// Constructors
	public TestOverrideAbstract(); // 0x100BF000

	// Methods
	public override void AbstractMethod(); // 0x100C5530
}

// Namespace: Il2CppTests.TestSources
internal class TestHideAbstractOverride : TestOverrideAbstract // TypeDefIndex: 1817
{
	// Constructors
	public TestHideAbstractOverride(); // 0x100BF000

	// Methods
	public new void AbstractMethod(); // 0x100C5530
}

// Namespace: Il2CppTests.TestSources
internal class TestHideVirtualAndNewVirtual : Test // TypeDefIndex: 1818
{
	// Constructors
	public TestHideVirtualAndNewVirtual(); // 0x100BF000

	// Methods
	public virtual new void VirtualMethod(); // 0x100C5530
}

// Namespace: Il2CppTests.TestSources
internal class TestHideOverrideAndNewVirtual : TestOverride // TypeDefIndex: 1819
{
	// Constructors
	public TestHideOverrideAndNewVirtual(); // 0x100BF000

	// Methods
	public virtual new void VirtualMethod(); // 0x100C5530
}

// Namespace: Il2CppTests.TestSources
internal abstract class TestAbstractNew : TestOverride // TypeDefIndex: 1820
{
	// Constructors
	protected TestAbstractNew(); // 0x100BF000

	// Methods
	public abstract new void VirtualMethod();
}

// Namespace: Il2CppTests.TestSources
internal class TestNewNonVirtualMethod : Test // TypeDefIndex: 1821
{
	// Constructors
	public TestNewNonVirtualMethod(); // 0x100BF000

	// Methods
	public new int ValueTypeReturnMethod(); // 0x100EF660
}

