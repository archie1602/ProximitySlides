using System.Globalization;

using CsvHelper;
using CsvHelper.Configuration;

namespace ProximitySlides.App.Metrics;

public class BleMetricsHandler
{
    private static readonly string MetricBasePath = Path.Combine(FileSystem.Current.AppDataDirectory, "ble_metrics.csv");

    public static void SaveMetrics(BleMetric metric)
    {
        AppendMetric(metric);

        //if (File.Exists(MetricBasePath))
        //{
        //    AppendMetric(metric);
        //    return;
        //}

        //CreateFirstRow(metric);
    }

    private static void CreateFirstRow(BleMetric metric)
    {
        using (var writer = new StreamWriter(MetricBasePath))
        {
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecord(metric);
            }
        }
    }

    private static void AppendMetric(BleMetric metric)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = !File.Exists(MetricBasePath)
        };

        using (var stream = File.Open(MetricBasePath, FileMode.Append))
        {
            using (var writer = new StreamWriter(stream))
            {
                using (var csv = new CsvWriter(writer, config))
                {
                    csv.WriteRecords(new List<BleMetric> { metric });
                }
            }
        }
    }
}
