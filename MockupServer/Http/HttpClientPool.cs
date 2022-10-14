using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Threading;

namespace MockupServer.Http
{
    public class HttpClientPool : IDisposable
    {
        ConcurrentQueue<HttpClient> _pool = new ConcurrentQueue<HttpClient>();
        int _maxSize = 10;
        int _current = 0;

        public HttpClientPool()
        {
        }
        public HttpClientPool(int maxSize)
        {
            _maxSize = maxSize;
        }

        public void Dispose()
        {
            _maxSize = 10;
            _current = 0;
            while (_pool.TryDequeue(out var context))
            {
                context.Dispose();
            }
        }

        public HttpClient GetHttpClient()
        {
            if (_pool.TryDequeue(out var context))
            {
                Interlocked.Decrement(ref _current);
                return context;
            }

            return new HttpClient();
        }

        public void Return(HttpClient context)
        {
            if (Interlocked.Increment(ref _current) <= _maxSize)
            {
                context.DefaultRequestHeaders.Clear();
                _pool.Enqueue(context);
            }
            else
            {
                context.Dispose();
                Interlocked.Decrement(ref _current);
            }
        }

    }
}
