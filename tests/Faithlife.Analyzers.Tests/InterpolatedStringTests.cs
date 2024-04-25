using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;

namespace Faithlife.Analyzers.Tests;

[TestFixture]
public sealed class InterpolatedStringTests : DiagnosticVerifier
{
	[Test]
	public void ValidInterpolatedStrings()
	{
		const string validProgram = @"
namespace TestApplication
{
	public class TestClass
	{
		public TestClass()
		{
			string one = ""${hello}"";
			string two = $""{one}"";
			string numberStr = $""{1}"";
			string floatStr = $""{1.0:0.00}"";
			string charStr = $""{'x'}"";
			string stringConcatStr = $""{""x"" + ""y""}"";
		}
	}
}";
		VerifyCSharpDiagnostic(validProgram);
	}

	[Test]
	public void ValidDollarSign()
	{
		const string validProgram = @"
namespace TestApplication
{
	public class TestClass
	{
		public TestClass()
		{
			string one = ""one"";
			string two = $""{one} costs $0.00"";
		}
	}
}";
		VerifyCSharpDiagnostic(validProgram);
	}

	[Test]
	public void ValidStringBuilder()
	{
		const string validProgram = @"
using System;
using System.Text;
using System.Collections.Generic;
namespace TestApplication
{
	public class TestClass
	{
		public TestClass()
		{
			var builder = new StringBuilder();
			var items = new List<(string Item1, TestClass2 Item2)>
			{
				(""one"", new TestClass2 { FileName = ""f1.txt"" }),
				(""three"", null),
			};
			builder.Append($""{items[0].Item1}:{items[0].Item2.FileName}"");
			builder.Insert(0, $""{items[0].Item1}:{items[0].Item2.FileName}"");
			builder.AppendLine($""{items[0].Item1}:{items[0].Item2.FileName}"");
			builder.AppendJoin($""{','} "", new[] { $""{items[1].Item1}"", $""{items[1].Item1}"" });
		}
	}
	public class TestClass2
	{
		public string FileName { get; set; }
	}
}";
		VerifyCSharpDiagnostic(validProgram);
	}

	[Test]
	public void InvalidInterpolatedString()
	{
		const string invalidProgram = @"
namespace TestApplication
{
	public class TestClass
	{
		public TestClass()
		{
			string one = ""${hello}"";
			string two = $""${one}"";
		}
	}
}";
		VerifyCSharpDiagnostic(invalidProgram, new DiagnosticResult
		{
			Id = InterpolatedStringAnalyzer.DiagnosticIdDollar,
			Message = "Avoid using ${} in interpolated strings.",
			Severity = DiagnosticSeverity.Warning,
			Locations = new[] { new DiagnosticResultLocation("Test0.cs", 9, 20) },
		});
	}

	[Test]
	public void InvalidInterpolatedStrings()
	{
		const string invalidProgram = @"
namespace TestApplication
{
	public class TestClass
	{
		public TestClass()
		{
			string one = ""${hello}"";
			string two = $""${one}${one}"";
		}
	}
}";
		VerifyCSharpDiagnostic(invalidProgram,
			new DiagnosticResult
			{
				Id = InterpolatedStringAnalyzer.DiagnosticIdDollar,
				Message = "Avoid using ${} in interpolated strings.",
				Severity = DiagnosticSeverity.Warning,
				Locations = new[] { new DiagnosticResultLocation("Test0.cs", 9, 20) },
			},
			new DiagnosticResult
			{
				Id = InterpolatedStringAnalyzer.DiagnosticIdDollar,
				Message = "Avoid using ${} in interpolated strings.",
				Severity = DiagnosticSeverity.Warning,
				Locations = new[] { new DiagnosticResultLocation("Test0.cs", 9, 26) },
			});
	}

	[Test]
	public void ConsecutiveInterpolatedStrings()
	{
		const string invalidProgram = @"
namespace TestApplication
{
	public class TestClass
	{
		public TestClass()
		{
			string one = ""${hello}"";
			string two = $""${one}{one}"";
		}
	}
}";
		VerifyCSharpDiagnostic(invalidProgram, new DiagnosticResult
		{
			Id = InterpolatedStringAnalyzer.DiagnosticIdDollar,
			Message = "Avoid using ${} in interpolated strings.",
			Severity = DiagnosticSeverity.Warning,
			Locations = new[] { new DiagnosticResultLocation("Test0.cs", 9, 20) },
		});
	}

	[Test]
	public void UnnecessaryInterpolatedString()
	{
		const string invalidProgram = @"
namespace TestApplication
{
	public class TestClass
	{
		public TestClass()
		{
			string str = $""Hello World"";
			string stringConcatStr = $""{$""x"" + ""y""}"";
		}
	}
}";
		VerifyCSharpDiagnostic(invalidProgram, new DiagnosticResult
		{
			Id = InterpolatedStringAnalyzer.DiagnosticIdUnnecessary,
			Message = "Avoid using an interpolated string where an equivalent literal string exists.",
			Severity = DiagnosticSeverity.Warning,
			Locations = new[] { new DiagnosticResultLocation("Test0.cs", 8, 17) },
		}, new DiagnosticResult
		{
			Id = InterpolatedStringAnalyzer.DiagnosticIdUnnecessary,
			Message = "Avoid using an interpolated string where an equivalent literal string exists.",
			Severity = DiagnosticSeverity.Warning,
			Locations = new[] { new DiagnosticResultLocation("Test0.cs", 9, 32) },
		});
	}

	[Test]
	public void EmptyInterpolatedString()
	{
		const string invalidProgram = @"
namespace TestApplication
{
	public class TestClass
	{
		public TestClass()
		{
			string str = $"""";
		}
	}
}";
		VerifyCSharpDiagnostic(invalidProgram, new DiagnosticResult
		{
			Id = InterpolatedStringAnalyzer.DiagnosticIdUnnecessary,
			Message = "Avoid using an interpolated string where an equivalent literal string exists.",
			Severity = DiagnosticSeverity.Warning,
			Locations = new[] { new DiagnosticResultLocation("Test0.cs", 8, 17) },
		});
	}

	protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new InterpolatedStringAnalyzer();
}
