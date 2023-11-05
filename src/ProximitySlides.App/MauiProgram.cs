using System.Reflection;
using CommunityToolkit.Maui;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ProximitySlides.App.Configuration;
using ProximitySlides.App.Managers.Listeners;
using ProximitySlides.App.Managers.Speakers;
using ProximitySlides.Core;
using Syncfusion.Maui.Core.Hosting;

namespace ProximitySlides.App;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .ConfigureSyncfusionCore()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        builder.AddAppSettings();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        builder.Services.AddApp(builder.Configuration);
        builder.Services.AddCore();

        builder.Services.AddSingleton<IProximitySender, BleSpeaker>();
        builder.Services.AddSingleton<IProximityListener, BleListener>();

        var app = builder.Build();

        Task.Run(async () =>
        {
            var rootDir = "pdfjs";
            var pathToRootDir = Path.Combine(FileSystem.Current.AppDataDirectory, rootDir);

            if (Directory.Exists(pathToRootDir))
            {
                Directory.Delete(pathToRootDir, true);
            }

            var pdfJsDirPaths = CreatePdfJsFoldersInAppDataDirectory(rootDir);

            foreach (var filePath in pdfJsDirPaths)
            {
                await CopyFileToAppDataDirectory(filePath);
            }
        })
        .Wait();

        return app;
    }

    private static void AddAppSettings(this MauiAppBuilder builder)
    {
        builder.AddJsonSettings("appsettings.json");

#if DEBUG
        builder.AddJsonSettings("appsettings.development.json");
#endif

#if !DEBUG
        builder.AddJsonSettings("appsettings.production.json");
#endif
    }

    private static void AddJsonSettings(this MauiAppBuilder builder, string fileName)
    {
        using var stream = Assembly
                               .GetExecutingAssembly()
                               .GetManifestResourceStream("ProximitySlides.App.appsettings.json") ??
                           throw new InvalidOperationException("Couldn't found appsettings configuration file");

        var config = new ConfigurationBuilder()
            .AddJsonStream(stream)
            .Build();

        builder.Configuration.AddConfiguration(config);
    }

    private static IEnumerable<string> CreatePdfJsFoldersInAppDataDirectory(string rootDir)
    {
        var pdfJsDirPaths = new[]
        {
            "index.html",
            "index.js",
            "index_old.html",
            "LICENSE",
            "assets/Introduction_to_Hadoop_slides.pdf",
            "build/pdf.js",
            "build/pdf.js.map",
            "build/pdf.sandbox.js",
            "build/pdf.sandbox.js.map",
            "build/pdf.worker.js",
            "build/pdf.worker.js.map",
            "web/compressed.tracemonkey-pldi-09.pdf",
            "web/debugger.css",
            "web/debugger.js",
            "web/viewer.css",
            "web/viewer.html",
            "web/viewer.js",
            "web/viewer.js.map",
            "web/cmaps/78-EUC-H.bcmap",
            "web/cmaps/78-EUC-V.bcmap",
            "web/cmaps/78-H.bcmap",
            "web/cmaps/78-RKSJ-H.bcmap",
            "web/cmaps/78-RKSJ-V.bcmap",
            "web/cmaps/78-V.bcmap",
            "web/cmaps/78ms-RKSJ-H.bcmap",
            "web/cmaps/78ms-RKSJ-V.bcmap",
            "web/cmaps/83pv-RKSJ-H.bcmap",
            "web/cmaps/90ms-RKSJ-H.bcmap",
            "web/cmaps/90ms-RKSJ-V.bcmap",
            "web/cmaps/90msp-RKSJ-H.bcmap",
            "web/cmaps/90msp-RKSJ-V.bcmap",
            "web/cmaps/90pv-RKSJ-H.bcmap",
            "web/cmaps/90pv-RKSJ-V.bcmap",
            "web/cmaps/Add-H.bcmap",
            "web/cmaps/Add-RKSJ-H.bcmap",
            "web/cmaps/Add-RKSJ-V.bcmap",
            "web/cmaps/Add-V.bcmap",
            "web/cmaps/Adobe-CNS1-0.bcmap",
            "web/cmaps/Adobe-CNS1-1.bcmap",
            "web/cmaps/Adobe-CNS1-2.bcmap",
            "web/cmaps/Adobe-CNS1-3.bcmap",
            "web/cmaps/Adobe-CNS1-4.bcmap",
            "web/cmaps/Adobe-CNS1-5.bcmap",
            "web/cmaps/Adobe-CNS1-6.bcmap",
            "web/cmaps/Adobe-CNS1-UCS2.bcmap",
            "web/cmaps/Adobe-GB1-0.bcmap",
            "web/cmaps/Adobe-GB1-1.bcmap",
            "web/cmaps/Adobe-GB1-2.bcmap",
            "web/cmaps/Adobe-GB1-3.bcmap",
            "web/cmaps/Adobe-GB1-4.bcmap",
            "web/cmaps/Adobe-GB1-5.bcmap",
            "web/cmaps/Adobe-GB1-UCS2.bcmap",
            "web/cmaps/Adobe-Japan1-0.bcmap",
            "web/cmaps/Adobe-Japan1-1.bcmap",
            "web/cmaps/Adobe-Japan1-2.bcmap",
            "web/cmaps/Adobe-Japan1-3.bcmap",
            "web/cmaps/Adobe-Japan1-4.bcmap",
            "web/cmaps/Adobe-Japan1-5.bcmap",
            "web/cmaps/Adobe-Japan1-6.bcmap",
            "web/cmaps/Adobe-Japan1-UCS2.bcmap",
            "web/cmaps/Adobe-Korea1-0.bcmap",
            "web/cmaps/Adobe-Korea1-1.bcmap",
            "web/cmaps/Adobe-Korea1-2.bcmap",
            "web/cmaps/Adobe-Korea1-UCS2.bcmap",
            "web/cmaps/B5-H.bcmap",
            "web/cmaps/B5-V.bcmap",
            "web/cmaps/B5pc-H.bcmap",
            "web/cmaps/B5pc-V.bcmap",
            "web/cmaps/CNS-EUC-H.bcmap",
            "web/cmaps/CNS-EUC-V.bcmap",
            "web/cmaps/CNS1-H.bcmap",
            "web/cmaps/CNS1-V.bcmap",
            "web/cmaps/CNS2-H.bcmap",
            "web/cmaps/CNS2-V.bcmap",
            "web/cmaps/ETen-B5-H.bcmap",
            "web/cmaps/ETen-B5-V.bcmap",
            "web/cmaps/ETenms-B5-H.bcmap",
            "web/cmaps/ETenms-B5-V.bcmap",
            "web/cmaps/ETHK-B5-H.bcmap",
            "web/cmaps/ETHK-B5-V.bcmap",
            "web/cmaps/EUC-H.bcmap",
            "web/cmaps/EUC-V.bcmap",
            "web/cmaps/Ext-H.bcmap",
            "web/cmaps/Ext-RKSJ-H.bcmap",
            "web/cmaps/Ext-RKSJ-V.bcmap",
            "web/cmaps/Ext-V.bcmap",
            "web/cmaps/GB-EUC-H.bcmap",
            "web/cmaps/GB-EUC-V.bcmap",
            "web/cmaps/GB-H.bcmap",
            "web/cmaps/GB-V.bcmap",
            "web/cmaps/GBK-EUC-H.bcmap",
            "web/cmaps/GBK-EUC-V.bcmap",
            "web/cmaps/GBK2K-H.bcmap",
            "web/cmaps/GBK2K-V.bcmap",
            "web/cmaps/GBKp-EUC-H.bcmap",
            "web/cmaps/GBKp-EUC-V.bcmap",
            "web/cmaps/GBpc-EUC-H.bcmap",
            "web/cmaps/GBpc-EUC-V.bcmap",
            "web/cmaps/GBT-EUC-H.bcmap",
            "web/cmaps/GBT-EUC-V.bcmap",
            "web/cmaps/GBT-H.bcmap",
            "web/cmaps/GBT-V.bcmap",
            "web/cmaps/GBTpc-EUC-H.bcmap",
            "web/cmaps/GBTpc-EUC-V.bcmap",
            "web/cmaps/H.bcmap",
            "web/cmaps/Hankaku.bcmap",
            "web/cmaps/Hiragana.bcmap",
            "web/cmaps/HKdla-B5-H.bcmap",
            "web/cmaps/HKdla-B5-V.bcmap",
            "web/cmaps/HKdlb-B5-H.bcmap",
            "web/cmaps/HKdlb-B5-V.bcmap",
            "web/cmaps/HKgccs-B5-H.bcmap",
            "web/cmaps/HKgccs-B5-V.bcmap",
            "web/cmaps/HKm314-B5-H.bcmap",
            "web/cmaps/HKm314-B5-V.bcmap",
            "web/cmaps/HKm471-B5-H.bcmap",
            "web/cmaps/HKm471-B5-V.bcmap",
            "web/cmaps/HKscs-B5-H.bcmap",
            "web/cmaps/HKscs-B5-V.bcmap",
            "web/cmaps/Katakana.bcmap",
            "web/cmaps/KSC-EUC-H.bcmap",
            "web/cmaps/KSC-EUC-V.bcmap",
            "web/cmaps/KSC-H.bcmap",
            "web/cmaps/KSC-Johab-H.bcmap",
            "web/cmaps/KSC-Johab-V.bcmap",
            "web/cmaps/KSC-V.bcmap",
            "web/cmaps/KSCms-UHC-H.bcmap",
            "web/cmaps/KSCms-UHC-HW-H.bcmap",
            "web/cmaps/KSCms-UHC-HW-V.bcmap",
            "web/cmaps/KSCms-UHC-V.bcmap",
            "web/cmaps/KSCpc-EUC-H.bcmap",
            "web/cmaps/KSCpc-EUC-V.bcmap",
            "web/cmaps/LICENSE",
            "web/cmaps/NWP-H.bcmap",
            "web/cmaps/NWP-V.bcmap",
            "web/cmaps/RKSJ-H.bcmap",
            "web/cmaps/RKSJ-V.bcmap",
            "web/cmaps/Roman.bcmap",
            "web/cmaps/UniCNS-UCS2-H.bcmap",
            "web/cmaps/UniCNS-UCS2-V.bcmap",
            "web/cmaps/UniCNS-UTF16-H.bcmap",
            "web/cmaps/UniCNS-UTF16-V.bcmap",
            "web/cmaps/UniCNS-UTF32-H.bcmap",
            "web/cmaps/UniCNS-UTF32-V.bcmap",
            "web/cmaps/UniCNS-UTF8-H.bcmap",
            "web/cmaps/UniCNS-UTF8-V.bcmap",
            "web/cmaps/UniGB-UCS2-H.bcmap",
            "web/cmaps/UniGB-UCS2-V.bcmap",
            "web/cmaps/UniGB-UTF16-H.bcmap",
            "web/cmaps/UniGB-UTF16-V.bcmap",
            "web/cmaps/UniGB-UTF32-H.bcmap",
            "web/cmaps/UniGB-UTF32-V.bcmap",
            "web/cmaps/UniGB-UTF8-H.bcmap",
            "web/cmaps/UniGB-UTF8-V.bcmap",
            "web/cmaps/UniJIS-UCS2-H.bcmap",
            "web/cmaps/UniJIS-UCS2-HW-H.bcmap",
            "web/cmaps/UniJIS-UCS2-HW-V.bcmap",
            "web/cmaps/UniJIS-UCS2-V.bcmap",
            "web/cmaps/UniJIS-UTF16-H.bcmap",
            "web/cmaps/UniJIS-UTF16-V.bcmap",
            "web/cmaps/UniJIS-UTF32-H.bcmap",
            "web/cmaps/UniJIS-UTF32-V.bcmap",
            "web/cmaps/UniJIS-UTF8-H.bcmap",
            "web/cmaps/UniJIS-UTF8-V.bcmap",
            "web/cmaps/UniJIS2004-UTF16-H.bcmap",
            "web/cmaps/UniJIS2004-UTF16-V.bcmap",
            "web/cmaps/UniJIS2004-UTF32-H.bcmap",
            "web/cmaps/UniJIS2004-UTF32-V.bcmap",
            "web/cmaps/UniJIS2004-UTF8-H.bcmap",
            "web/cmaps/UniJIS2004-UTF8-V.bcmap",
            "web/cmaps/UniJISPro-UCS2-HW-V.bcmap",
            "web/cmaps/UniJISPro-UCS2-V.bcmap",
            "web/cmaps/UniJISPro-UTF8-V.bcmap",
            "web/cmaps/UniJISX0213-UTF32-H.bcmap",
            "web/cmaps/UniJISX0213-UTF32-V.bcmap",
            "web/cmaps/UniJISX02132004-UTF32-H.bcmap",
            "web/cmaps/UniJISX02132004-UTF32-V.bcmap",
            "web/cmaps/UniKS-UCS2-H.bcmap",
            "web/cmaps/UniKS-UCS2-V.bcmap",
            "web/cmaps/UniKS-UTF16-H.bcmap",
            "web/cmaps/UniKS-UTF16-V.bcmap",
            "web/cmaps/UniKS-UTF32-H.bcmap",
            "web/cmaps/UniKS-UTF32-V.bcmap",
            "web/cmaps/UniKS-UTF8-H.bcmap",
            "web/cmaps/UniKS-UTF8-V.bcmap",
            "web/cmaps/V.bcmap",
            "web/cmaps/WP-Symbol.bcmap",
            "web/images/altText_add.svg",
            "web/images/altText_done.svg",
            "web/images/annotation-check.svg",
            "web/images/annotation-comment.svg",
            "web/images/annotation-help.svg",
            "web/images/annotation-insert.svg",
            "web/images/annotation-key.svg",
            "web/images/annotation-newparagraph.svg",
            "web/images/annotation-noicon.svg",
            "web/images/annotation-note.svg",
            "web/images/annotation-paperclip.svg",
            "web/images/annotation-paragraph.svg",
            "web/images/annotation-pushpin.svg",
            "web/images/cursor-editorFreeText.svg",
            "web/images/cursor-editorInk.svg",
            "web/images/findbarButton-next.svg",
            "web/images/findbarButton-previous.svg",
            "web/images/gv-toolbarButton-download.svg",
            "web/images/gv-toolbarButton-openinapp.svg",
            "web/images/loading-dark.svg",
            "web/images/loading-icon.gif",
            "web/images/loading.svg",
            "web/images/secondaryToolbarButton-documentProperties.svg",
            "web/images/secondaryToolbarButton-firstPage.svg",
            "web/images/secondaryToolbarButton-handTool.svg",
            "web/images/secondaryToolbarButton-lastPage.svg",
            "web/images/secondaryToolbarButton-rotateCcw.svg",
            "web/images/secondaryToolbarButton-rotateCw.svg",
            "web/images/secondaryToolbarButton-scrollHorizontal.svg",
            "web/images/secondaryToolbarButton-scrollPage.svg",
            "web/images/secondaryToolbarButton-scrollVertical.svg",
            "web/images/secondaryToolbarButton-scrollWrapped.svg",
            "web/images/secondaryToolbarButton-selectTool.svg",
            "web/images/secondaryToolbarButton-spreadEven.svg",
            "web/images/secondaryToolbarButton-spreadNone.svg",
            "web/images/secondaryToolbarButton-spreadOdd.svg",
            "web/images/toolbarButton-bookmark.svg",
            "web/images/toolbarButton-currentOutlineItem.svg",
            "web/images/toolbarButton-download.svg",
            "web/images/toolbarButton-editorFreeText.svg",
            "web/images/toolbarButton-editorInk.svg",
            "web/images/toolbarButton-editorStamp.svg",
            "web/images/toolbarButton-menuArrow.svg",
            "web/images/toolbarButton-openFile.svg",
            "web/images/toolbarButton-pageDown.svg",
            "web/images/toolbarButton-pageUp.svg",
            "web/images/toolbarButton-presentationMode.svg",
            "web/images/toolbarButton-print.svg",
            "web/images/toolbarButton-search.svg",
            "web/images/toolbarButton-secondaryToolbarToggle.svg",
            "web/images/toolbarButton-sidebarToggle.svg",
            "web/images/toolbarButton-viewAttachments.svg",
            "web/images/toolbarButton-viewLayers.svg",
            "web/images/toolbarButton-viewOutline.svg",
            "web/images/toolbarButton-viewThumbnail.svg",
            "web/images/toolbarButton-zoomIn.svg",
            "web/images/toolbarButton-zoomOut.svg",
            "web/images/treeitem-collapsed.svg",
            "web/images/treeitem-expanded.svg",
            "web/locale/locale.properties",
            "web/locale/ach/viewer.properties",
            "web/locale/af/viewer.properties",
            "web/locale/an/viewer.properties",
            "web/locale/ar/viewer.properties",
            "web/locale/ast/viewer.properties",
            "web/locale/az/viewer.properties",
            "web/locale/be/viewer.properties",
            "web/locale/bg/viewer.properties",
            "web/locale/bn/viewer.properties",
            "web/locale/bo/viewer.properties",
            "web/locale/br/viewer.properties",
            "web/locale/brx/viewer.properties",
            "web/locale/bs/viewer.properties",
            "web/locale/ca/viewer.properties",
            "web/locale/cak/viewer.properties",
            "web/locale/ckb/viewer.properties",
            "web/locale/cs/viewer.properties",
            "web/locale/cy/viewer.properties",
            "web/locale/da/viewer.properties",
            "web/locale/de/viewer.properties",
            "web/locale/dsb/viewer.properties",
            "web/locale/el/viewer.properties",
            "web/locale/en-CA/viewer.properties",
            "web/locale/en-GB/viewer.properties",
            "web/locale/en-US/viewer.properties",
            "web/locale/eo/viewer.properties",
            "web/locale/es-AR/viewer.properties",
            "web/locale/es-CL/viewer.properties",
            "web/locale/es-ES/viewer.properties",
            "web/locale/es-MX/viewer.properties",
            "web/locale/et/viewer.properties",
            "web/locale/eu/viewer.properties",
            "web/locale/fa/viewer.properties",
            "web/locale/ff/viewer.properties",
            "web/locale/fi/viewer.properties",
            "web/locale/fr/viewer.properties",
            "web/locale/fur/viewer.properties",
            "web/locale/fy-NL/viewer.properties",
            "web/locale/ga-IE/viewer.properties",
            "web/locale/gd/viewer.properties",
            "web/locale/gl/viewer.properties",
            "web/locale/gn/viewer.properties",
            "web/locale/gu-IN/viewer.properties",
            "web/locale/he/viewer.properties",
            "web/locale/hi-IN/viewer.properties",
            "web/locale/hr/viewer.properties",
            "web/locale/hsb/viewer.properties",
            "web/locale/hu/viewer.properties",
            "web/locale/hy-AM/viewer.properties",
            "web/locale/hye/viewer.properties",
            "web/locale/ia/viewer.properties",
            "web/locale/id/viewer.properties",
            "web/locale/is/viewer.properties",
            "web/locale/it/viewer.properties",
            "web/locale/ja/viewer.properties",
            "web/locale/ka/viewer.properties",
            "web/locale/kab/viewer.properties",
            "web/locale/kk/viewer.properties",
            "web/locale/km/viewer.properties",
            "web/locale/kn/viewer.properties",
            "web/locale/ko/viewer.properties",
            "web/locale/lij/viewer.properties",
            "web/locale/lo/viewer.properties",
            "web/locale/lt/viewer.properties",
            "web/locale/ltg/viewer.properties",
            "web/locale/lv/viewer.properties",
            "web/locale/meh/viewer.properties",
            "web/locale/mk/viewer.properties",
            "web/locale/mr/viewer.properties",
            "web/locale/ms/viewer.properties",
            "web/locale/my/viewer.properties",
            "web/locale/nb-NO/viewer.properties",
            "web/locale/ne-NP/viewer.properties",
            "web/locale/nl/viewer.properties",
            "web/locale/nn-NO/viewer.properties",
            "web/locale/oc/viewer.properties",
            "web/locale/pa-IN/viewer.properties",
            "web/locale/pl/viewer.properties",
            "web/locale/pt-BR/viewer.properties",
            "web/locale/pt-PT/viewer.properties",
            "web/locale/rm/viewer.properties",
            "web/locale/ro/viewer.properties",
            "web/locale/ru/viewer.properties",
            "web/locale/sat/viewer.properties",
            "web/locale/sc/viewer.properties",
            "web/locale/scn/viewer.properties",
            "web/locale/sco/viewer.properties",
            "web/locale/si/viewer.properties",
            "web/locale/sk/viewer.properties",
            "web/locale/skr/viewer.properties",
            "web/locale/sl/viewer.properties",
            "web/locale/son/viewer.properties",
            "web/locale/sq/viewer.properties",
            "web/locale/sr/viewer.properties",
            "web/locale/sv-SE/viewer.properties",
            "web/locale/szl/viewer.properties",
            "web/locale/ta/viewer.properties",
            "web/locale/te/viewer.properties",
            "web/locale/tg/viewer.properties",
            "web/locale/th/viewer.properties",
            "web/locale/tl/viewer.properties",
            "web/locale/tr/viewer.properties",
            "web/locale/trs/viewer.properties",
            "web/locale/uk/viewer.properties",
            "web/locale/ur/viewer.properties",
            "web/locale/uz/viewer.properties",
            "web/locale/vi/viewer.properties",
            "web/locale/wo/viewer.properties",
            "web/locale/xh/viewer.properties",
            "web/locale/zh-CN/viewer.properties",
            "web/locale/zh-TW/viewer.properties",
            "web/standard_fonts/FoxitDingbats.pfb",
            "web/standard_fonts/FoxitFixed.pfb",
            "web/standard_fonts/FoxitFixedBold.pfb",
            "web/standard_fonts/FoxitFixedBoldItalic.pfb",
            "web/standard_fonts/FoxitFixedItalic.pfb",
            "web/standard_fonts/FoxitSerif.pfb",
            "web/standard_fonts/FoxitSerifBold.pfb",
            "web/standard_fonts/FoxitSerifBoldItalic.pfb",
            "web/standard_fonts/FoxitSerifItalic.pfb",
            "web/standard_fonts/FoxitSymbol.pfb",
            "web/standard_fonts/LiberationSans-Bold.ttf",
            "web/standard_fonts/LiberationSans-BoldItalic.ttf",
            "web/standard_fonts/LiberationSans-Italic.ttf",
            "web/standard_fonts/LiberationSans-Regular.ttf",
            "web/standard_fonts/LICENSE_FOXIT",
            "web/standard_fonts/LICENSE_LIBERATION"
        };

        pdfJsDirPaths = pdfJsDirPaths
            .Select(it => Path.Combine(rootDir, it))
            .ToArray();

        var pdfJsDirectories = pdfJsDirPaths
            .Select(Path.GetDirectoryName)
            .Where(it => !string.IsNullOrWhiteSpace(it))
            .Distinct()
            .ToList();

        foreach (var d in pdfJsDirectories)
        {
            if (string.IsNullOrWhiteSpace(d))
            {
                continue;
            }

            var fullDirPath = Path.Combine(FileSystem.Current.AppDataDirectory, d);

            if (!Directory.Exists(fullDirPath))
            {
                Directory.CreateDirectory(fullDirPath);
            }
        }

        return pdfJsDirPaths;
    }

    private static async Task CopyFileToAppDataDirectory(string filename)
    {
        // Open the source file
        using var inputStream = await FileSystem.Current.OpenAppPackageFileAsync(filename);

        // Create an output filename
        var targetFile = Path.Combine(FileSystem.Current.AppDataDirectory, filename);
        
        if (!File.Exists(targetFile))
        {
            // Copy the file to the AppDataDirectory
            using var outputStream = File.Create(targetFile);
            await inputStream.CopyToAsync(outputStream);
        }
    }
}