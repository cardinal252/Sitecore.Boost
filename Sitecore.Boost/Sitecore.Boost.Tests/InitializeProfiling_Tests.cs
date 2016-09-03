using System;
using System.Diagnostics;
using System.IO;
using NUnit.Framework;
using Sitecore.Diagnostics;
using Sitecore.Mvc.Pipelines.Response.RenderRendering;
using Sitecore.Mvc.Presentation;
using Sitecore.Pipelines;

namespace Sitecore.Boost.Tests
{
    [TestFixture]
    public class InitializeProfiling_Tests
    {
        [TestFixtureSetUp]
        public void Setup()
        {
            Sitecore.Configuration.State.HttpRuntime.AppDomainAppPath = Directory.GetCurrentDirectory();
            CorePipeline.Run("initialize", new PipelineArgs());
            Console.WriteLine("Initialized");
        }

        [Test]
        public void InitializeNewProfiling()
        {
            InitializeProfiling profiling = new InitializeProfiling();
            Rendering rendering = new Rendering();
            var args1 = new RenderRenderingArgs(rendering, null) { Cacheable = true };
            Tracer.StartSession();
            Stopwatch sw = new Stopwatch();
            sw.Start();
            for (var i = 0; i < 100000; i++)
            {
                profiling.Process(args1);
            }
            sw.Stop();
            Console.WriteLine("Elapsed {0}", sw.ElapsedMilliseconds);
            Tracer.EndSession();
            //Console.WriteLine(Tracer.GetFormattedTrace("trace.xslt"));
        }

        [Test]
        public void InitializeOldProfiling()
        {
            InitializeProfiling profiling = new InitializeProfiling();
            Rendering rendering = new Rendering();
            var args1 = new RenderRenderingArgs(rendering, null) { Cacheable = true };
            // warm up
            profiling.Process(args1);
            
            Tracer.StartSession();
            Stopwatch sw = new Stopwatch();
            sw.Start();
            for (var i = 0; i < 100000; i++)
            {
                profiling.Process(args1);
            }
            sw.Stop();
            Console.WriteLine("Elapsed {0}", sw.ElapsedMilliseconds);
            Tracer.EndSession();
            //Console.WriteLine(Tracer.GetFormattedTrace("trace.xslt"));
        }
    }
}
