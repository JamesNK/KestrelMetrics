using System.Net;

namespace KestrelMetrics.Web;

public class SlowStreamingHttpContext : HttpContent
{
    protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context)
    {
        throw new NotImplementedException();
    }

    protected override async Task SerializeToStreamAsync(Stream stream, TransportContext? context, CancellationToken cancellationToken)
    {
        for (var i = 0; i < 1000; i++)
        {
            await stream.WriteAsync(new byte[1]);
            await stream.FlushAsync(cancellationToken);
            await Task.Delay(TimeSpan.FromSeconds(1));
        }
    }

    protected override bool TryComputeLength(out long length)
    {
        length = 0;
        return false;
    }
}
