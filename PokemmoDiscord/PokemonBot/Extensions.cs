namespace PokemmoDiscord.PokemonBot
{
    public static class Extensions
    {
        public static string NumericString(this int x, byte digitsleft = 3)
        {
            var result = x.ToString();
            while (result.Length < digitsleft)
                result = "0" + result;
            return result;
        }
    }
}
