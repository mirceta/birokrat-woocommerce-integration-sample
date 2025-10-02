using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace common_birowoo
{
    public class SimpleDecoratingFactory<TDep1, TDecoratee>
    {
        private List<Func<TDep1, TDecoratee, TDecoratee>> decorators;

        public SimpleDecoratingFactory(Func<TDep1, TDecoratee, TDecoratee> initialDecorator)
        {
            decorators = new List<Func<TDep1, TDecoratee, TDecoratee>> { initialDecorator };
        }

        public void AddDecorator(Func<TDep1, TDecoratee, TDecoratee> newDecorator)
        {
            decorators.Add(newDecorator);
        }

        public TDecoratee Decorate(TDep1 integration, TDecoratee nextAccessor)
        {
            foreach (var decorator in decorators)
            {
                nextAccessor = decorator(integration, nextAccessor);
            }

            return nextAccessor;
        }
    }

    public class DecoratingFactory<TDecoratee>
    {
        private List<Func<Dictionary<string, object>, TDecoratee, Task<TDecoratee>>> decorators;

        public DecoratingFactory(Func<Dictionary<string, object>, TDecoratee, Task<TDecoratee>> initialDecorator)
        {
            decorators = new List<Func<Dictionary<string, object>, TDecoratee, Task<TDecoratee>>> { initialDecorator };
        }

        public void AddDecorator(Func<Dictionary<string, object>, TDecoratee, Task<TDecoratee>> newDecorator)
        {
            decorators.Add(newDecorator);
        }

        public async Task<TDecoratee> Decorate(Dictionary<string, object> integration, TDecoratee nextAccessor)
        {
            foreach (var decorator in decorators)
            {
                nextAccessor = await decorator(integration, nextAccessor);
            }

            return nextAccessor;
        }
    }
}
