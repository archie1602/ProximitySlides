namespace ProximitySlides.App.Helpers;

public class GoogleDriveHelper
{
    public static async Task<Dictionary<int, string>> UploadMock(string presentationName, bool needReAuthorize = false)
    {
        var links = new Dictionary<int, string>();

        switch (presentationName)
        {
            case "CSharp dotnet basics.pdf":

                links = new Dictionary<int, string>
                {
                    { 1, "https://drive.google.com/uc?id=1DB1BeeHDcTJlUFk_XlydaQiJq5XOTzNh" },
                    { 2, "https://drive.google.com/uc?id=1gObh1L2IK1xqigVK-PMxyBGBrk4FqAog" },
                    { 3, "https://drive.google.com/uc?id=17j5W3BoyCOLZ9WMXcOiOgqR3gOCa86XN" },
                    { 4, "https://drive.google.com/uc?id=1_BaPWmx-DyKoOvM3ML2scY51KoyMqIPz" },
                    { 5, "https://drive.google.com/uc?id=1fHkDx4JDqEOhVc_c0BToUw_tPd9xZ2P0" },
                    { 6, "https://drive.google.com/uc?id=1Gbz1PgASuCI9da2-4JEw0nNPrzgZs8je" },
                    { 7, "https://drive.google.com/uc?id=1IZ2LLqzKL5zCfJam1jQGa8h1F3cr7qQu" },
                    { 8, "https://drive.google.com/uc?id=1OPf2lkOvnks-_42JUHBCbGDH5U6Ny_x5" },
                    { 9, "https://drive.google.com/uc?id=1t9AUfksu6XiTmJ4t30Q9yndzo52OhKaO" },
                    { 10, "https://drive.google.com/uc?id=1Amn8OTBL1_SCXimTs1CPHNwxONf2Ag58" },
                    { 11, "https://drive.google.com/uc?id=1t7tyOhFHai3Qj57ke3MCETfsTD0GDN1-" },
                    { 12, "https://drive.google.com/uc?id=1uVNRP2fffwCj89wdWI7KWe4jxaBO1H6k" },
                    { 13, "https://drive.google.com/uc?id=1HpLe6Cl4dMfi2dBjqNvEUJ8sRdNe2RYr" },
                    { 14, "https://drive.google.com/uc?id=10GSWXaKfT6K6Z_ax-qZFvtRvqbj-uOXg" },
                    { 15, "https://drive.google.com/uc?id=13vfnmxA-9xLawO6M5Ao9dh8U0r7ei-Uh" }
                };

                break;

            case "getting_started_docker.pdf":

                links = new Dictionary<int, string>
                {
                    { 1, "aSDbsdhsh6733hds8nbr8ner85bnf8nr84nf8n48ng8j58fngf85nf88gn58gngjjf85nf88gn5n898dnajdsds9ds7n548485nf" },
                    { 2, "a2" },
                    { 3, "a3" },
                    { 4, "a4" },
                    { 5, "a5" },
                    { 6, "a6" },
                    { 7, "a7" }
                };

                break;

            default:

                links = new Dictionary<int, string>
                {
                    { 1, "https://drive.google.com/uc?id=1qaCK7zcZYCGrelBSuO26y5ExDSwIf6g-" },
                    { 2, "https://drive.google.com/uc?id=1cY_8eEsgmsiWtiyGUBFyvBqUwgdXCxc_" },
                    { 3, "https://drive.google.com/uc?id=1HkVf0Bz6XPsZUvXSue24ygzDpQdjJ2Gf" },
                    { 4, "https://drive.google.com/uc?id=1oFyjDPPcRly_GlvCuRy7vH4XV6EwQBwy" },
                    { 5, "https://drive.google.com/uc?id=10ndZlbwalDAEA9HjM6LN_7adFwZV56sv" },
                    { 6, "https://drive.google.com/uc?id=1YQupP3ww4OGI5SpOFM_rY4t8PNQ9JJgE" },
                    { 7, "https://drive.google.com/uc?id=1EJcp_NY1pvsxE4pBLwsrXpUnO1N7_bDj" },
                    { 8, "https://drive.google.com/uc?id=1AuS9rVS1nNIqZ-8zf4bzQ6-xrvROE6Vc" },
                    { 9, "https://drive.google.com/uc?id=1qOl5eYj_Vna78bXdXjvFK6xCwGuYJpJP" },
                    { 10, "https://drive.google.com/uc?id=1RVAkkBfBiTa81TPrUvuqSJG9pRora--l" }
                };

                break;
        }

        return links;
    }
}
