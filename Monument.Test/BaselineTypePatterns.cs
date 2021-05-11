using Microsoft.VisualStudio.TestTools.UnitTesting;
using Monument.SimpleInjector;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Monument.Test
{
    [TestClass]
    public class BaselineTypePatterns
    {
        public static Type GetType<T>(params Type[] types)
            where T : class => Get<T>(types).GetType();

        public static T Get<T>(params Type[] types)
            where T : class => new TypePatternRegistrationConvention()
                .Register(types, new ContainerAdapter(new Container()))
                .ToRuntimeContainer()
                .Resolve<T>();

        [TestMethod]
        public void BasicRegistration()
        => Assert.AreEqual(typeof(SimpleImplementation1),
            GetType<ISimpleInterface>(
                typeof(SimpleImplementation1)));

        [TestMethod]
        public void BasicDecoratorRegistration()
         => Assert.AreEqual(typeof(SimpleDecorator),
             GetType<ISimpleInterface>(
                 typeof(SimpleImplementation1),
                 typeof(SimpleDecorator)));

        [TestMethod]
        public void BasicCompositeRegistration()
         => Assert.AreEqual(typeof(SimpleComposite),
             GetType<ISimpleInterface>(
                 typeof(SimpleComposite),
                 typeof(SimpleImplementation1),
                 typeof(SimpleImplementation2),
                 typeof(SimpleImplementation3)));

        [TestMethod]
        public void OpenGenericRegistration()
         => Assert.AreEqual(typeof(OpenGenericImplementation1<string>),
             GetType<IGenericInterface<string>>(
                 typeof(OpenGenericImplementation1<>)));

        [TestMethod]
        public void GetTypeKeyReturnsCorrectSignature() =>
            Assert.AreEqual(typeof(IGenericInterface<>), typeof(IGenericInterface<string>).ToTypeKey());

        [TestMethod]
        public void OpenGenericDecoratorRegistration()
         => Assert.AreEqual(typeof(OpenGenericDecorator<string>),
             GetType<IGenericInterface<string>>(
                 typeof(OpenGenericDecorator<>),
                 typeof(OpenGenericImplementation1<>)));

        [TestMethod]
        public void OpenGenericDecoratorWithClosedDecoratorRegistration()
        {
            var instance = Get<IGenericInterface<SimpleImplementation1>>(
                typeof(OpenGenericDecorator<>),
                typeof(ClosedGenericDecorator),
                typeof(ClosedGenericImplementation1));

            var decorator1 = instance as OpenGenericDecorator<SimpleImplementation1> ?? (instance as ClosedGenericDecorator).Inner as OpenGenericDecorator<SimpleImplementation1>;
            var decorator2 = instance as ClosedGenericDecorator ?? (instance as OpenGenericDecorator<SimpleImplementation1>).Inner as ClosedGenericDecorator;

            Assert.IsNotNull(decorator1);
            Assert.IsNotNull(decorator2);
        }

        [TestMethod]
        public void MixedCollectionOfOpenAndClosedImplementations()
        {
            var instances = Get<IEnumerable<IGenericInterface<SimpleImplementation1>>>(
                typeof(OpenGenericImplementation1<>),
                typeof(ClosedGenericImplementation1));

            var instance2s = Get<IEnumerable<IGenericInterface<SimpleImplementation2>>>(
                typeof(OpenGenericImplementation1<>),
                typeof(ClosedGenericImplementation1));

            Assert.AreEqual(1, instance2s.Count());
            Assert.AreEqual(2, instances.Count());
        }

        [TestMethod]
        public void OpenGenericCompositeAroundMixedCollectionOfOpenAndClosedImplementations() =>
            Assert.AreEqual(typeof(OpenGenericComposite<SimpleImplementation1>), GetType<IGenericInterface<SimpleImplementation1>>(
                typeof(OpenGenericComposite<>),
                typeof(OpenGenericImplementation1<>),
                typeof(ClosedGenericImplementation1)));

        [TestMethod]
        public void CompositeAroundMixedCollectionOfOpenAndClosedImplementations() =>
            Assert.AreEqual(typeof(ClosedGenericComposite), GetType<IGenericInterface<SimpleImplementation1>>(
                typeof(ClosedGenericComposite),
                typeof(OpenGenericImplementation1<>),
                typeof(ClosedGenericImplementation1)));

        [TestMethod, Ignore]
        public void WhereDoesTheDecoratorHide()
        {
            var instance = Get<IGenericInterface<SimpleImplementation1>>(
                typeof(ClosedGenericComposite),
                typeof(OpenGenericImplementation1<>),
                typeof(ClosedGenericDecorator),
                typeof(ClosedGenericImplementation1));

            Assert.IsTrue(true);

            // that's a problem
        }

        [TestMethod]
        public void OpenGenericCompositeRegistration()
         => Assert.AreEqual(typeof(OpenGenericComposite<string>),
             GetType<IGenericInterface<string>>(
                 typeof(OpenGenericComposite<>),
                 typeof(OpenGenericImplementation1<>),
                 typeof(OpenGenericImplementation2<>),
                 typeof(OpenGenericImplementation3<>)));

        [TestMethod]
        public void ClosedAndOpenGenericListRegistration()
        {
            Assert.AreEqual(2, Get<IEnumerable<IGenericInterface<SimpleImplementation2>>>(
                typeof(OpenGenericImplementation1<>),
                typeof(ClosedGenericImplementation1),
                typeof(ClosedGenericImplementation4)).Count());
            Assert.AreEqual(2, Get<IEnumerable<IGenericInterface<SimpleImplementation1>>>(
                typeof(OpenGenericImplementation1<>),
                typeof(ClosedGenericImplementation1),
                typeof(ClosedGenericImplementation4)).Count());
        }

        [TestMethod]
        public void ClosedGenericRegistration()
        {
            Assert.IsNotNull(Get<IGenericInterface<SimpleImplementation2>>(
                typeof(ClosedGenericImplementation1),
                typeof(ClosedGenericImplementation4)));
            Assert.IsNotNull(Get<IGenericInterface<SimpleImplementation1>>(
                typeof(ClosedGenericImplementation1),
                typeof(ClosedGenericImplementation4)));
        }

        [TestMethod]
        public void ClosedGenericListRegistration() =>
            Assert.AreEqual(2, Get<IEnumerable<IGenericInterface<SimpleImplementation1>>>(
                typeof(ClosedGenericImplementation1),
                typeof(ClosedGenericImplementation2)).Count());

        [TestMethod]
        public void ClosedGenericAdapterRegistration() =>
            Assert.AreEqual(typeof(ClosedGenericAdapter), GetType<IGenericInterface<SimpleImplementation1>>(
                typeof(ClosedGenericAdapter),
                typeof(ClosedGenericImplementation4)));
    }

    public class SimpleImplementation1 : ISimpleInterface { }
    public class SimpleImplementation2 : ISimpleInterface { }
    public class SimpleImplementation3 : ISimpleInterface { }
    public class OpenGenericImplementation1<T> : IGenericInterface<T> { }
    public class OpenGenericImplementation2<T> : IGenericInterface<T> { }
    public class OpenGenericImplementation3<T> : IGenericInterface<T> { }
    public class ClosedGenericImplementation1 : IGenericInterface<SimpleImplementation1> { }
    public class ClosedGenericImplementation2 : IGenericInterface<SimpleImplementation1> { }
    public class ClosedGenericImplementation3 : IGenericInterface<SimpleImplementation1> { }
    public class ClosedGenericImplementation4 : IGenericInterface<SimpleImplementation2> { }
    public class SimpleComposite : ISimpleInterface
    {
        public IEnumerable<ISimpleInterface> Inner { get; }
        public SimpleComposite(IEnumerable<ISimpleInterface> d) { Inner = d; }
    }
    public class OpenGenericComposite<T> : IGenericInterface<T>
    {
        public IEnumerable<IGenericInterface<T>> Inner { get; }
        public OpenGenericComposite(IEnumerable<IGenericInterface<T>> d) { Inner = d; }
    }
    public class ClosedGenericComposite : IGenericInterface<SimpleImplementation1>
    {
        public IEnumerable<IGenericInterface<SimpleImplementation1>> Inner { get; }
        public ClosedGenericComposite(IEnumerable<IGenericInterface<SimpleImplementation1>> d) { Inner = d; }
    }
    public class SimpleDecorator : ISimpleInterface
    {
        public ISimpleInterface Inner { get; }
        public SimpleDecorator(ISimpleInterface d) { Inner = d; }
    }
    public class OpenGenericDecorator<T> : IGenericInterface<T>
    {
        public IGenericInterface<T> Inner { get; }

        public OpenGenericDecorator(IGenericInterface<T> d) { Inner = d; }
    }
    public class ClosedGenericDecorator : IGenericInterface<SimpleImplementation1>
    {
        public IGenericInterface<SimpleImplementation1> Inner { get; }

        public ClosedGenericDecorator(IGenericInterface<SimpleImplementation1> d) { this.Inner = d; }
    }
    public interface ISimpleInterface { }
    public interface IGenericInterface<T> { }

    public class ClosedGenericAdapter : IGenericInterface<SimpleImplementation1>
    {
        public ClosedGenericAdapter(IGenericInterface<SimpleImplementation2> inner) { Inner = inner; }

        public IGenericInterface<SimpleImplementation2> Inner { get; }
    }
}
