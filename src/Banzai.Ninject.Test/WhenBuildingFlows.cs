﻿using System;
using Banzai.Factories;
using Banzai.Ninject;
using Ninject;
using NUnit.Framework;
using Should;

namespace Banzai.Ninject.Test
{
    [TestFixture]
    public class WhenBuildingFlows
    {
        [Test]
        public void Simple_Flow_Is_Registered_With_Container()
        {
            var kernel = new StandardKernel();
            kernel.RegisterBanzaiNodes(GetType().Assembly, true);

            var flowBuilder = new FlowBuilder<object>(new NinjectFlowRegistrar(kernel));

            flowBuilder.CreateFlow("TestFlow1")
                .AddRoot<IPipelineNode<object>>()
                .AddChild<ITestNode2>();

            flowBuilder.Register();

            var flow = kernel.Get<FlowComponent<object>>("TestFlow1");

            flow.ShouldNotBeNull();
            flow.IsFlow.ShouldBeTrue();
        }

        [Test]
        public void Simple_Flow_Is_Accessed_With_NodeFactory()
        {
            var kernel = new StandardKernel();
            kernel.RegisterBanzaiNodes(GetType().Assembly, true);

            var flowBuilder = new FlowBuilder<object>(new NinjectFlowRegistrar(kernel));

            flowBuilder.CreateFlow("TestFlow1")
                .AddRoot<IPipelineNode<object>>()
                .AddChild<ITestNode2>();

            flowBuilder.Register();

            var factory = kernel.Get<INodeFactory<object>>();

            var flow = factory.GetFlow("TestFlow1");

            flow.ShouldNotBeNull();
        }

        [Test]
        public void Simple_Flow_Contains_All_Nodes()
        {
            var kernel = new StandardKernel();
            kernel.RegisterBanzaiNodes(GetType().Assembly, true);

            var flowBuilder = new FlowBuilder<object>(new NinjectFlowRegistrar(kernel));

            flowBuilder.CreateFlow("TestFlow1")
                .AddRoot<PipelineNode<object>>()
                .AddChild<ITestNode2>()
                .AddChild<IPipelineNode<object>>()
                .ForChild<IPipelineNode<object>>()
                .AddChild<ITestNode4>()
                .AddChild<ITestNode3>()
                .AddChild<ITestNode2>();
                
            flowBuilder.Register();

            var factory = kernel.Get<INodeFactory<object>>();

            var flow = (IPipelineNode<object>)factory.GetFlow("TestFlow1");

            flow.ShouldBeType<PipelineNode<object>>();
            flow.Children.Count.ShouldEqual(2);
            var subflow = (IPipelineNode<object>)flow.Children[1];
            subflow.Children.Count.ShouldEqual(3);
            subflow.Children[1].ShouldBeType<TestNode3>();
        }

        [Test]
        public void Multiple_Flows_Can_Be_Registered()
        {
            var kernel = new StandardKernel();
            kernel.RegisterBanzaiNodes(GetType().Assembly, true);

            var flowBuilder = new FlowBuilder<object>(new NinjectFlowRegistrar(kernel));

            flowBuilder.CreateFlow("TestFlow2")
                .AddRoot<IPipelineNode<object>>()
                .AddChild<ITestNode4>()
                .AddChild<ITestNode3>()
                .AddChild<ITestNode2>();

            flowBuilder.Register();

            flowBuilder.CreateFlow("TestFlow1")
                .AddRoot<IPipelineNode<object>>()
                .AddChild<ITestNode2>();

            flowBuilder.Register();

            var factory = kernel.Get<INodeFactory<object>>();

            var flow = factory.GetFlow("TestFlow1");
            flow.ShouldNotBeNull();

            flow = factory.GetFlow("TestFlow2");
            flow.ShouldNotBeNull();
        }

        [Test]
        public void Simple_Flow_Contains_Subflow()
        {
            var kernel = new StandardKernel();
            kernel.RegisterBanzaiNodes(GetType().Assembly, true);

            var flowBuilder = new FlowBuilder<object>(new NinjectFlowRegistrar(kernel));

            flowBuilder.CreateFlow("TestFlow2")
                .AddRoot<PipelineNode<object>>()
                .AddChild<ITestNode4>()
                .AddChild<ITestNode3>()
                .AddChild<ITestNode2>();

            flowBuilder.Register();

            flowBuilder.CreateFlow("TestFlow1")
                .AddRoot<PipelineNode<object>>()
                .AddChild<ITestNode2>()
                .AddFlow("TestFlow2");
                
            flowBuilder.Register();

            var factory = kernel.Get<INodeFactory<object>>();

            var flow = (IPipelineNode<object>)factory.GetFlow("TestFlow1");

            flow.ShouldBeType<PipelineNode<object>>();
            flow.Children.Count.ShouldEqual(2);
            var subflow = (IPipelineNode<object>)flow.Children[1];
            subflow.Children.Count.ShouldEqual(3);
            subflow.Children[1].ShouldBeType<TestNode3>();
        }

        [Test]
        public void Adding_Child_Node_To_Simple_Node_Errs()
        {
            var kernel = new StandardKernel();
            kernel.RegisterBanzaiNodes(GetType().Assembly, true);

            var flowBuilder = new FlowBuilder<object>(new NinjectFlowRegistrar(kernel));

            var componentBuilder = flowBuilder.CreateFlow("TestFlow")
                .AddRoot<ITestNode4>();

            Assert.Throws<InvalidOperationException>(() => componentBuilder.AddChild<ITestNode4>());

        }

        [Test]
        public void Adding_Child_Flow_To_Simple_Node_Errs()
        {
            var kernel = new StandardKernel();
            kernel.RegisterBanzaiNodes(GetType().Assembly, true);

            var flowBuilder = new FlowBuilder<object>(new NinjectFlowRegistrar(kernel));

            flowBuilder.CreateFlow("TestFlow2")
                .AddRoot<IPipelineNode<object>>()
                .AddChild<ITestNode4>()
                .AddChild<ITestNode3>()
                .AddChild<ITestNode2>();

            flowBuilder.Register();

            var componentBuilder = flowBuilder.CreateFlow("TestFlow")
                .AddRoot<ITestNode4>();

            Assert.Throws<InvalidOperationException>(() => componentBuilder.AddFlow("TestFlow2"));

        }

    }
}