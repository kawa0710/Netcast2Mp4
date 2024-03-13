namespace RayITUtilityCore31
{
    public static class StringExtension
    {
        public static string Left(this string param, int length)
        {
            string result = param.Substring(0, length);
            return result;
        }

        public static string Right(this string param, int length)
        {
            string result = param.Substring(param.Length - length, length);
            return result;
        }

        public static string Mid(this string param, int startIndex, int length)
        {
            string result = param.Substring(startIndex, length);
            return result;
        }

        public static string Mid(this string param, int startIndex)
        {
            string result = param.Substring(startIndex);
            return result;
        }
    }
}
