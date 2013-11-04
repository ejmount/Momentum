using System;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
sealed class MenuItem : Attribute
{
	public MenuItem(string Name)
	{
		this.Name = Name;
	}

	public string Name
	{
		get;
		set; 
	}

	public int Order { get; set; }
}