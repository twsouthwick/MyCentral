using Microsoft.AspNetCore.Components;
using System;

namespace MyCentral.Browser.Shared
{
    public class BlazorObserver<TItem> : ComponentBase, IDisposable, IObserver<TItem>
    {
        [Parameter]
        public IObservable<TItem> Observable { get; set; }

        private IDisposable _disposable;

        protected override void OnParametersSet()
        {
            if (_disposable is not null)
            {
                _disposable.Dispose();
            }

            if (Observable is not null)
            {
                _disposable = Observable.Subscribe(this);
            }
        }

        public void Dispose()
        {
            _disposable?.Dispose();
        }

        public virtual void OnCompleted()
        {

        }

        public virtual void OnError(Exception error)
        {
        }

        public virtual void OnNext(TItem value)
        {
        }
    }
}
