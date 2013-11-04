using System;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
sealed class MenuItem : Attribute
{
	public MenuItem(string Name, int Order)
	{
		this.Name = Name;
		this.Order = Order;
	}

	public string Name
	{
		get;
		set; 
	}

	public int Order { get; set; }
}