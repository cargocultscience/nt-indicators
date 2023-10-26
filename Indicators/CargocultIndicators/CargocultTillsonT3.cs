//
// Copyright (C) 2023, NinjaTrader LLC <www.ninjatrader.com>.
// NinjaTrader reserves the right to modify or overwrite this NinjaScript component with each release.
//
#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds indicators in this folder and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators
{
	/// <summary>
	/// Tillson T3
	/// </summary>
	
	public class CargocultTillsonT3 : Indicator
	{
		class EMA
		{
			double priorWeight;
			double currentWeight;
			public double value { get; set; }
			public EMA(int periods) 
			{
				currentWeight = 2.0 / (1 + periods);
				priorWeight = 1 - currentWeight;
			}
			public void update(double v)
			{
				value = v * currentWeight + value * priorWeight;
			}
		}

		public class Tillson
		{
			private EMA ema1;
			private EMA ema2;
			private EMA ema3;
			private EMA ema4;
			private EMA ema5;
			private EMA ema6;
			private double volumeFactor;
			public double value;
			public Tillson(int periods, double volumeFactor)
			{
				ema1 = new EMA(periods);
				ema2 = new EMA(periods);
				ema3 = new EMA(periods);
				ema4 = new EMA(periods);
				ema5 = new EMA(periods);
				ema6 = new EMA(periods);
				this.volumeFactor = volumeFactor;
			}
			public double update(double v)
			{
				ema1.update(v);
				ema2.update(ema1.value);
				ema3.update(ema2.value);
				ema4.update(ema3.value);
				ema5.update(ema4.value);
				ema6.update(ema5.value);
				// todo move these static calcs to class variables
				double c1 = -volumeFactor * volumeFactor * volumeFactor;
				double c2 = 3 * volumeFactor * volumeFactor + 3 * volumeFactor * volumeFactor * volumeFactor;
				double c3 = -6 * volumeFactor * volumeFactor - 3 * volumeFactor - 3 * volumeFactor * volumeFactor * volumeFactor;
				double c4 = 1 + 3 * volumeFactor + volumeFactor * volumeFactor * volumeFactor + 3 * volumeFactor * volumeFactor;
				value = c1 * ema6.value + c2 * ema5.value + c3 * ema4.value + c4 * ema3.value;
				return value;
			}
		}
		
		private String version;
		private Tillson t3;
		private Tillson fibo;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= NinjaTrader.Custom.Resource.NinjaScriptIndicatorDescriptionEMA;
				Name						= "Cargocult Tillson T3";
				IsOverlay					= true;
				IsSuspendedWhileInactive	= true;
				T3Periods						= 8;
				T3VolumeFactor				= 0.7;
				T3LineWidth        = 2;
				T3LineOpacity 		= 75;
				T3LineType         = DashStyleHelper.Solid;
				EnableFiboLine = true;
				FiboLineWidth        = 2;
				FiboLineOpacity 		= 75;
				FiboLineType         = DashStyleHelper.Dot;
				FiboPeriods          = 5;
				FiboVolumeFactor = 0.618;


			}
			else if (State == State.Configure)
			{
				Log(String.Format("Cargocult Tillson T3 version {0}", version), LogLevel.Information);
				t3 = new Tillson(T3Periods, T3VolumeFactor);
				if(EnableFiboLine)
				{
					fibo = new Tillson(FiboPeriods, FiboVolumeFactor);
				}
				AddPlot(new Stroke(Brushes.MediumTurquoise, T3LineType, T3LineWidth, T3LineOpacity), PlotStyle.Line, "T3 Line");
				AddPlot(new Stroke(EnableFiboLine ? Brushes.MediumTurquoise : Brushes.Transparent, FiboLineType, FiboLineWidth, FiboLineOpacity), PlotStyle.Line, "Fibo Line");
			}
		}

		protected override void OnBarUpdate()
		{
			double weightedPrice = (High[0] + Low[0] + 2 * Close[0]) / 4.0;

			if(CurrentBar == 0)
			{
				T3[0] = weightedPrice;
				Fibo[0] = weightedPrice;
				return;
			}
			
			T3[0] = t3.update(weightedPrice);
			if(EnableFiboLine)
			{
				Fibo[0] = fibo.update(weightedPrice);
			}

			// TODO make colors config

			PlotBrushes[0][0] = T3[0] > T3[1] ? Brushes.Green : Brushes.Red;
			PlotBrushes[1][0] = Fibo[0] > Fibo[1] ? Brushes.Cyan : Brushes.Purple;
		}

		#region Properties
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> T3
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Fibo
		{
			get { return Values[1]; }
		}
		
		[Display(Name = "T3 Periods", Description = "", Order = 1, GroupName="Parameters")]
		public int T3Periods
		{ get; set; }

		[Display(Name = "Volume Factor", Description = "", Order = 2, GroupName="Parameters")]
		public double T3VolumeFactor
		{ get; set; }

		[Range(1, int.MaxValue)]
		[Display(Name="T3 Line Width", Description="", Order=20, GroupName="T3 Line Parameters")]
		public int T3LineWidth
		{ get; set; }

		[Range(1, 100)]
		[Display(Name="T3 Line Opacity", Description="", Order=21, GroupName="T3 Line Parameters")]
		public int T3LineOpacity
		{ get; set; }

		[Display(Name="T3 Line Type", Description="", Order=22, GroupName="T3 Line Parameters")]
		public DashStyleHelper T3LineType
		{ get; set; }

		[Display(Name="Enable Fibo Line", Description="", Order=30, GroupName="Fibo Line Parameters")]
		public bool EnableFiboLine
		{ get; set; }

		[Display(Name="Fibo Periods", Description="", Order=30, GroupName="Fibo Line Parameters")]
		public int FiboPeriods
		{ get; set; }

		[Display(Name="Fibo Volume Factor", Description="", Order=31, GroupName="Fibo Line Parameters")]
		public double FiboVolumeFactor
		{ get; set; }

		[Range(1, int.MaxValue)]
		[Display(Name="Fibo Line Width", Description="", Order=32, GroupName="Fibo Line Parameters")]
		public int FiboLineWidth
		{ get; set; }

		[Range(1, 100)]
		[Display(Name="Fibo Line Opacity", Description="", Order=33, GroupName="Fibo Line Parameters")]
		public int FiboLineOpacity
		{ get; set; }

		[Display(Name="Fibo Line Type", Description="", Order=34, GroupName="Fibo Line Parameters")]
		public DashStyleHelper FiboLineType
		{ get; set; }

		#endregion

		
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private CargocultTillsonT3[] cacheCargocultTillsonT3;
		public CargocultTillsonT3 CargocultTillsonT3()
		{
			return CargocultTillsonT3(Input);
		}

		public CargocultTillsonT3 CargocultTillsonT3(ISeries<double> input)
		{
			if (cacheCargocultTillsonT3 != null)
				for (int idx = 0; idx < cacheCargocultTillsonT3.Length; idx++)
					if (cacheCargocultTillsonT3[idx] != null &&  cacheCargocultTillsonT3[idx].EqualsInput(input))
						return cacheCargocultTillsonT3[idx];
			return CacheIndicator<CargocultTillsonT3>(new CargocultTillsonT3(), input, ref cacheCargocultTillsonT3);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.CargocultTillsonT3 CargocultTillsonT3()
		{
			return indicator.CargocultTillsonT3(Input);
		}

		public Indicators.CargocultTillsonT3 CargocultTillsonT3(ISeries<double> input )
		{
			return indicator.CargocultTillsonT3(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.CargocultTillsonT3 CargocultTillsonT3()
		{
			return indicator.CargocultTillsonT3(Input);
		}

		public Indicators.CargocultTillsonT3 CargocultTillsonT3(ISeries<double> input )
		{
			return indicator.CargocultTillsonT3(input);
		}
	}
}

#endregion
