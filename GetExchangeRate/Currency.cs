namespace GetExchangeRate
{
    internal class Currency
    {
        internal string Name { get; set; }

        internal string Code { get; set; }

        internal int Nominal { get; set; }

        internal decimal Rate { get; set; }

        internal Currency(string name, string code, int nominal, decimal rate)
        {
            Name = name;
            Code = code;
            Nominal = nominal;
            Rate = rate;
        }
    }
}
