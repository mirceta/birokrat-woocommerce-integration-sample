using System;
using System.Collections.Generic;

namespace tests_exec_dynamictask.deps
{
    /*
    Implement the observer pattern. The below class should be the subject. The purpose of the observable will be to post changes in the
        integration source to the subscribers. It should publish an event in the Run() method where the comments are.
        It should also expose a method that will allow consumers of this object to subscribe to the observable!
    */


    public class Notifier : IObservable<string>
    {
        private List<IObserver<string>> observers = new List<IObserver<string>>();

        public void Notify(string change)
        {
            foreach (var observer in observers)
            {
                observer.OnNext(change);
            }
        }

        public IDisposable Subscribe(IObserver<string> observer)
        {
            if (!observers.Contains(observer))
                observers.Add(observer);
            return new Unsubscriber(observers, observer);
        }
    }
}
