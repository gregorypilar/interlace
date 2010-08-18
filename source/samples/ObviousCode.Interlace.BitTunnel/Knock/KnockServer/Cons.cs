using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KnockServer
{
    public class Cons<T, U>
    {
        public Cons(T car, U cdr)
        {
            Car = car;
            Cdr = cdr;
        }

        public T Car;
        public U Cdr;
    }

    public class Cons : Cons<object, object> 
    {
        public Cons(object car, object cdr) : base(car, cdr)
        {

        }
    }
}
