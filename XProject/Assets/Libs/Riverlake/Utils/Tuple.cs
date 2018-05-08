using System;
using System.Collections;

public static class Tuple
{
	public static Tuple<T1> Create<T1>(T1 item1) { return new Tuple<T1>() { Item1 = item1 }; }
	public static Tuple<T1, T2> Create<T1, T2>(T1 item1, T2 item2) { return new Tuple<T1, T2>() { Item1 = item1, Item2 = item2 }; }
	public static Tuple<T1, T2, T3> Create<T1, T2, T3>(T1 item1, T2 item2, T3 item3) { return new Tuple<T1, T2, T3>() { Item1 = item1, Item2 = item2, Item3 = item3 }; }
	public static Tuple<T1, T2, T3, T4> Create<T1, T2, T3, T4>(T1 item1, T2 item2, T3 item3, T4 item4) { return new Tuple<T1, T2, T3, T4>() { Item1 = item1, Item2 = item2, Item3 = item3, Item4 = item4 }; }
	public static Tuple<T1, T2, T3, T4, T5> Create<T1, T2, T3, T4, T5>(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5) { return new Tuple<T1, T2, T3, T4, T5>() { Item1 = item1, Item2 = item2, Item3 = item3, Item4 = item4, Item5 = item5 }; }
	public static Tuple<T1, T2, T3, T4, T5, T6> Create<T1, T2, T3, T4, T5, T6>(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6) { return new Tuple<T1, T2, T3, T4, T5, T6>() { Item1 = item1, Item2 = item2, Item3 = item3, Item4 = item4, Item5 = item5, Item6 = item6 }; }
	public static Tuple<T1, T2, T3, T4, T5, T6, T7> Create<T1, T2, T3, T4, T5, T6, T7>(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7) { return new Tuple<T1, T2, T3, T4, T5, T6, T7>() { Item1 = item1, Item2 = item2, Item3 = item3, Item4 = item4, Item5 = item5, Item6 = item6, Item7 = item7 }; }
}

public class Tuple<T1>
{
	public T1 Item1 { get; set; }

	public override string ToString()
	{
		return string.Format("{{{0}}}", Item1);
	}
}

public class Tuple<T1, T2>
{
	public T1 Item1 { get; set; }
	public T2 Item2 { get; set; }

	public override string ToString()
	{
		return string.Format("{{{0}, {1}}}", Item1, Item2);
	}
}

public class Tuple<T1, T2, T3>
{
	public T1 Item1 { get; set; }
	public T2 Item2 { get; set; }
	public T3 Item3 { get; set; }

	public override string ToString()
	{
		return string.Format("{{{0}, {1}, {2}}}", Item1, Item2, Item3);
	}
}

public class Tuple<T1, T2, T3, T4>
{
	public T1 Item1 { get; set; }
	public T2 Item2 { get; set; }
	public T3 Item3 { get; set; }
	public T4 Item4 { get; set; }

	public override string ToString()
	{
		return string.Format("{{{0}, {1}, {2}, {3}}}", Item1, Item2, Item3, Item4);
	}
}

public class Tuple<T1, T2, T3, T4, T5>
{
	public T1 Item1 { get; set; }
	public T2 Item2 { get; set; }
	public T3 Item3 { get; set; }
	public T4 Item4 { get; set; }
	public T5 Item5 { get; set; }

	public override string ToString()
	{
		return string.Format("{{{0}, {1}, {2}, {3}, {4}}}", Item1, Item2, Item3, Item4, Item5);
	}
}

public class Tuple<T1, T2, T3, T4, T5, T6>
{
	public T1 Item1 { get; set; }
	public T2 Item2 { get; set; }
	public T3 Item3 { get; set; }
	public T4 Item4 { get; set; }
	public T5 Item5 { get; set; }
	public T6 Item6 { get; set; }

	public override string ToString()
	{
		return string.Format("{{{0}, {1}, {2}, {3}, {4}, {5}}}", Item1, Item2, Item3, Item4, Item5, Item6);
	}
}

public class Tuple<T1, T2, T3, T4, T5, T6, T7>
{
	public T1 Item1 { get; set; }
	public T2 Item2 { get; set; }
	public T3 Item3 { get; set; }
	public T4 Item4 { get; set; }
	public T5 Item5 { get; set; }
	public T6 Item6 { get; set; }
	public T7 Item7 { get; set; }

	public override string ToString()
	{
		return string.Format("{{{0}, {1}, {2}, {3}, {4}, {5}, {6}}}", Item1, Item2, Item3, Item4, Item5, Item6, Item7);
	}
}
