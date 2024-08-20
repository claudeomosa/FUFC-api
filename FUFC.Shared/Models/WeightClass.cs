namespace FUFC.Shared.Models
{
    public static class WeightClass
    {
        public static readonly string FlyWeight = "Flyweight";
        public static readonly string BantamWeight = "Bantamweight";
        public static readonly string FeatherWeight = "Featherweight";
        public static readonly string LightWeight = "Lightweight";
        public static readonly string WelterWeight = "Welterweight";
        public static readonly string MiddleWeight = "Middleweight";
        public static readonly string LightHeavyWeight = "Light Heavyweight";
        public static readonly string HeavyWeight = "Heavyweight";
        public static readonly string Unknown = "";

        public static string GetWeightClass(int weightInLbs)
        {
            return weightInLbs switch
            {
                <= 0 => Unknown,
                <= 125 => FlyWeight,
                <= 135 => BantamWeight,
                <= 145 => FeatherWeight,
                <= 155 => LightWeight,
                <= 170 => WelterWeight,
                <= 185 => MiddleWeight,
                <= 205 => LightHeavyWeight,
                <= 265 => HeavyWeight,
                _ => Unknown, // Beyond typical weight class ranges
            };
        }
    }
}