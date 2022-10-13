using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DfoToa.BatchRun
{
    internal class ArgumentUtility
    {
        private readonly string[] _args;
        private bool _needHelp;

        public ArgumentUtility(string[] args)
        {
            _args = args;
            Init();
        }

        public string? FromDate { get; private set; }
        public string? ToDate { get; private set; }
        public bool Silent { get; private set; }

        private void Init()
        {
            if (_args == null) return;


            var argsWithIndex = _args.Select((a, i) => new { i, a });
            var indexFromDate = argsWithIndex.Where(arg => arg.a.ToLower() == "-f" || arg.a.ToLower() == "--from").Select(arg => arg.i);
            var indexToDate = argsWithIndex.Where(arg => arg.a.ToLower() == "-t" || arg.a.ToLower() == "--to").Select(arg => arg.i);
            var indexSilent = argsWithIndex.Where(arg => arg.a.ToLower() == "-s" || arg.a.ToLower() == "--silent").Select(arg => arg.i);
            var indexHelp = argsWithIndex.Where(arg => arg.a.ToLower() == "-h" || arg.a.ToLower() == "--help").Select(arg => arg.i);
            _needHelp = indexHelp != null && indexHelp.Any();

            if (indexFromDate != null && indexFromDate.Any())
            {
                int index = indexFromDate.First();
                var range = index..(index + 2);
                FromDate = _args.Take(range).Last();
            }
            if (indexToDate != null && indexToDate.Any())
            {
                int index = indexToDate.First();
                var range = index..(index + 2);
                ToDate = _args.Take(range).Last();
            }
            else if (FromDate != null) ToDate = DateTimeOffset.Now.Date.ToString("yyyyMMdd");
            if (indexSilent != null && indexSilent.Any() && FromDate != null)
            {
                Silent = true;
            }
        }

        internal bool HelpNeeded()
        {
            if (!_needHelp) return false;
            Console.WriteLine("argumenter: [-f | --from <yyyymmdd>] [-t | --to <yyyymmdd>] [-s | --silent]");
            Console.WriteLine("-f, --from\tFra dato med format yyyymmdd");
            Console.WriteLine("-t, --to\tTil dato med format yyyymmdd");
            Console.WriteLine("-s, --silent\tEksekverer uten input fra kommando, krever at -f, --from er satt");
            return _needHelp;
        }
    }
}
