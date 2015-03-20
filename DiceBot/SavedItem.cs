using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiceBot
{
    public class SavedItem
    {
        public string Name { get; set; }
        public string Value { get; set; }

        public SavedItem(string _Name, string _Value)
        {
            Name = _Name;
            Value = _Value;
        }
    }
}
