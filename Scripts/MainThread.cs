using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Timberborn.SingletonSystem;
using UnityEngine;

namespace Mods.WebUI.Scripts {
  internal class MainThread : IUpdatableSingleton {
    private readonly ConcurrentQueue<Action> _queue = new ConcurrentQueue<Action>();

    public void UpdateSingleton() {
      if (!_queue.IsEmpty) {
        while(_queue.TryDequeue(out var action)) {
          action?.Invoke();
        }
      }
    }

    public async Task<T> Invoke<T>(Func<T> fn) {
      var acs = new AwaitableCompletionSource<T>();
      _queue.Enqueue(() => {
        acs.SetResult(fn());
      });
      return await acs.Awaitable;
    }

  }

}