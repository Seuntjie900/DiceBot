namespace DiceBot
{


    public class AppHelpers
    {
        public const string AppName = "DICEBot by WinMachine";
        public const string AppVersion = "4.1.19.0";


        public const string DateTimeCounterFormat = @"d'D, 'hh\:mm\:ss";

        public static string AppFullName
        {
            get
            {
                return AppName + " [" + AppVersion + "]" ;
            }
        }

    }

}
