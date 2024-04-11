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

            case "bench_test_presentation.pdf":

                links = new Dictionary<int, string>
                {
                    { 1, "51fdo" },
                    { 2, "10p556sz5h" },
                    { 3, "21t2f4fvhuc9mpmkth3oo" },
                    { 4, "2612ynud7mys8tahttki5fh8uk" },
                    { 5, "42n3p7kov4ikjt947ibgh5iy2y8er2g3rsuppx35ti" },
                    { 6, "47smfymu8db0rou7enonv459m2p34e19e24ozfam58fxuwh" },
                    { 7, "63mv2jxcnd756kxf2ftejhxxzwizwojh90gwr4qfwkbhc4pfzbt1yxbqopncujw" },
                    { 8, "68ckhtzeuw3cqnamuh8a8gfy8dd43hesspr21cyy9zk3tr9jthmxa2syhdcighiqiowb" },
                    { 9, "84j6un528ngw9x2cazh3eywy4cbuqy1473rnauxfke187hj73tusbabgtwre3hut4tdafk6wufsevry9ozh0" },
                    { 10, "89criuyfxz8p5uvic70pd0tsqnm5i6kmeyhmzangj6r0iutf43b1pi47ofzu1emtsh38k8xo2hv4m33r19ijxj7vq" },
                    { 11, "105zrg40ya7j1kcvadxus9dq2hx4eturuwhjx2j3suh4m1hftnib7d04vcj30cxfqkjetf72f3ajuaq71c2k6c5k0jzjycxwh6r4jupg4" },
                    { 12, "110052ph1qg69x774v21j4bxfmzx9nb1a85bxzui8stasidnsrbmgcq7y62t10ouof4qaotixjg4fmgyt2j9bvvcu948k7e3brcnof2j69mtgp" },
                    { 13, "1263gc6ybx1arfbc01bzpsx5p8po520sqo0oqkpqwrs6mtttxs73y2w3437pfh799kxr57okeo1j0zce22i29swxde5n0owihfk359k5h17zhnwmwxq0j6rus026zv" },
                    { 14, "1310vee3qqapg5ezqou7vr0k2inwutoa9rdtmt658iwq17su1sjp5oo94u693kpoy7vsncetcgi49ajpbn6skfwet2u6nf4f7bwxmsmizo133meq2wx74gci77umd7sjhyd" },
                    { 15, "147y6nqqgtf20n65fsh2ofyrrm3umr8bn7zjwh0dep2uevmjr3wijznru60x5ma4pwj0m7pkrbgt00p331wgmagus59j1cn09pg6ad9hmpxw1ntje91ha6ts3cnyo50qeokh8tr6i968xmt1975" },
                    { 16, "152ncyz8gintfp0q0u8qaoz35rn2frq2rx8k9eidv96r7ymvermgdumhg1o8isjf8j9w9twcdtpnwer7t6iaxic88hux95krx2staoxtt53n4dhb18bjx7qzy7d62fpmajtru9db9dfxd8ygf86yumb0" },
                    { 17, "1689io150iikucxhx6fk2pvr52pywjn7aqqoxhuknebpcw9szzhyx5e83eu15e1itkccsjzrg3utmrkc55jcfyjftrn0kgu9j1546a8kq2zgfv6pkbh0zyve93c8395myij3bj0pjoau99o0ka8hjj1pdpayu2i8m9k4fm8j" },
                    { 18, "173gvyvzhk90o81hb5xvhqozcp2d1yaf7io9b8ou4p3d6kgp6ukt5fc26euqzfxtsjnaz3y2g3jk0oe3ckub2q5zbj62cik0yx47byjwqixzcunuuz0nb4k7bno91sb67vuzm6426xvqtkf4cxd7xyv4c7nmnm9mmgkd7r0215djb" },
                    { 19, "189wi8ypbgoxqc1waj6dhhe2sc94wsxkrad9w9enfecd6vj04x990x70k84ucufy7r5hh8t9r8rmh276o1krztacqr52xi7uv5s1w2ujpmodwirssbbs7o0qv7v25dtuv8tn8pwt81qurhq693iazncx10i65hzdx4wy9d7upepc5966d5nyzgpy2woze" },
                    { 20, "194btm3odbfy49w43p14pzgo5uukykhxtb1urogvqp4agajuup7en020t54hdem3qqq62dk8xmaqsoi2nuu76yk06ooybixqqai8bec2sm3eaxx2kaz7bbqjh90py6wo1ch406272ptkp84ght08esaxioe707fffnyxftyv9y7pvjm00a1ybakkk4tevebgk2" },
                    { 21, "210j26270mbx2zhwq2a8jisuo0m2gsj8w9ji0t40o6cp6f5bma4e26xdgfggn808e03q9ww9hi6v6wrt100d0dr4tqnpszsdpp353ft8twz2rzq64uaokn09cxn6rayb2nkbztqee75mst6sjxdsqpzwhqzt6etxj4onpyug23vppu3wuqr5q6hkw7pprv8cxuvzww2vvg4a3a0anm" },

                    { 22, "a1" },
                    { 23, "a1" },
                    { 24, "a1" },
                    { 25, "a1" },
                    { 26, "a1" },
                    { 27, "a1" },
                    { 28, "a1" },
                    { 29, "a1" },
                    { 30, "a1" },
                    { 31, "a1" },
                    { 32, "a1" },
                    { 33, "a1" },
                    { 34, "a1" },
                    { 35, "a1" },
                    { 36, "a1" },
                    { 37, "a1" },
                    { 38, "a1" },
                    { 39, "a1" },
                    { 40, "a1" },
                    { 41, "a1" },
                    { 42, "a1" },
                    { 43, "a1" },
                    { 44, "a1" },
                    { 45, "a1" },
                    { 46, "a1" },
                    { 47, "a1" },
                    { 48, "a1" },
                    { 49, "a1" },
                    { 50, "a1" },
                    { 51, "a1" },
                    { 52, "a1" },
                    { 53, "a1" },
                    { 54, "a1" },
                    { 55, "a1" },
                    { 56, "a1" },
                    { 57, "a1" },
                    { 58, "a1" },
                    { 59, "a1" },
                    { 60, "a1" },
                    { 61, "a1" }
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
