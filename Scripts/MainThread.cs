using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Timberborn.SingletonSystem;
using UnityEngine;

namespace Mods.WebUI.Scripts {
  public class MainThread : IUpdatableSingleton {
    private readonly ConcurrentQueue<Action> _queue = new ConcurrentQueue<Action>();

    public void UpdateSingleton() {
      if (!_queue.IsEmpty) {
        while(_queue.TryDequeue(out var action)) {
          action?.Invoke();
        }
      }
    }

    public T Invoke<T>(Func<T> fn) {
      return InvokeAsync(fn).Result;
    }

    public async Task<T> InvokeAsync<T>(Func<T> fn) {
      var acs = new AwaitableCompletionSource<Tuple<T, Exception>>();
      _queue.Enqueue(() => {
        try {
          acs.SetResult(new Tuple<T, Exception>(fn(), null));
        }
        catch (Exception e) {
          acs.SetResult(new Tuple<T, Exception>(default, e));
        }
      });
      var result = await acs.Awaitable;
      if (result.Item2 != null) {
        throw new Exception("Error on MainThread", result.Item2);
      }
      return result.Item1;
    }

  }

}