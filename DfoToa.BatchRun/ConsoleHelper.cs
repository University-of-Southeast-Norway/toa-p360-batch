namespace DfoToa.BatchRun
{
    internal class ConsoleHelper
    {
        private readonly bool _silent;
        public ConsoleHelper(bool silent)
        {
            _silent = silent;
        }

        internal string? GetFromDate()
        {
            if (_silent) return null;
            Console.Write("Angi startdato (yyyymmdd): ");
            return Console.ReadLine();
        }

        internal string? GetToDate()
        {
            if (_silent) return null;
            Console.Write("Angi sluttdato (yyyymmdd): ");
            return Console.ReadLine();
        }

        internal bool Proceed(string message)
        {
            if (_silent) return true;
            Console.Write(message);
            return Console.ReadLine()?.ToLower()?.Contains("j") == true;
        }
    }
}
