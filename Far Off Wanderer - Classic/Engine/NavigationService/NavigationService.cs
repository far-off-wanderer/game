using System;
using System.Collections.Generic;
using System.Linq;

namespace Conesoft.Engine.NavigationService
{
    public interface INavigationService<T> where T : class
    {
        T Current { get; }
        void NavigateTo(Type type);
        void NavigateTo<T2>() where T2 : class, T;
        void GoBack();
        void GoHome();
    }

    namespace Implementation
    {
        class NavigationService<T> : INavigationService<T> where T : class
        {
            Stack<T> stack;

            public T Current { get { return stack.Count > 0 ? stack.Peek() : null; } }

            private TinyIoC.TinyIoCContainer Container { get { return TinyIoC.TinyIoCContainer.Current; } }

            public NavigationService()
            {
                stack = new Stack<T>();
            }

            public void NavigateTo(Type type)
            {
                stack.Push(Container.Resolve(type) as T);
            }

            public void NavigateTo<T2>() where T2 : class, T
            {
                stack.Push(Container.Resolve<T2>());
            }

            public void GoBack()
            {
                stack.Pop();
            }

            public void GoHome()
            {
                var newStack = new Stack<T>();
                newStack.Push(stack.First());
                stack = newStack;
            }
        }
    }
}
