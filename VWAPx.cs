#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
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

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	public class VWAP : Indicator
	{
		double	iCumVolume			= 0;
		double	iCumTypicalVolume	= 0;
		double curVWAP = 0;
		double deviation = 0;
		double v2Sum = 0;
		double hl3 = 0;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description							= @"Volume Weighted Average Price";
				Name								= "VWAPx";
				Calculate							= Calculate.OnBarClose;
				IsOverlay							= true;
				DisplayInDataBox					= true;
				DrawOnPricePanel					= true;
				DrawHorizontalGridLines				= true;
				DrawVerticalGridLines				= true;
				PaintPriceMarkers					= true;
				ScaleJustification					= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				NumDeviations 						= 4;
				SD1 								= 1.28;
				SD2 								= 2.01;
				SD3 								= 2.51;
				SD4 								= 3.1;
				SD5 								= 4.0;
				
				
				//Standard Deviation Area 1
				SD1AreaBrush					= Brushes.CornflowerBlue;
				SD1AreaOpacity					= 6;
				
				//Standard Deviation Area 2
				SD2AreaBrush					= Brushes.CornflowerBlue;
				SD2AreaOpacity					= 2;
				
				//Standard Deviation Area 3
				SD3AreaBrush					= Brushes.DarkOrange;
				SD3AreaOpacity					= 4;
				
				//Standard Deviation Area 4
				SD4AreaBrush					= Brushes.Brown;
				SD4AreaOpacity					= 4;
				
				//Standard Deviation Area 5
				SD5AreaBrush					= Brushes.Red;
				SD5AreaOpacity					= 5;
				
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive			= true;
				AddPlot(new Stroke(Brushes.Black,DashStyleHelper.Dot,1), PlotStyle.Line, "PlotVWAP");
				
				AddPlot(new Stroke(Brushes.Tan, DashStyleHelper.Dot,1), PlotStyle.Line, "PlotVWAP1U");
				AddPlot(new Stroke(Brushes.Tan, DashStyleHelper.Dot,1), PlotStyle.Line, "PlotVWAP1L");
				
				AddPlot(Brushes.Orange, "PlotVWAP2U");
				AddPlot(Brushes.Orange, "PlotVWAP2L");
				
				AddPlot(new Stroke(Brushes.Firebrick,1), PlotStyle.Line, "PlotVWAP3U");
				AddPlot(new Stroke(Brushes.Firebrick,1), PlotStyle.Line, "PlotVWAP3L");
				
				AddPlot(Brushes.Red, "PlotVWAP4U");
				AddPlot(Brushes.Red, "PlotVWAP4L");
				
				AddPlot(Brushes.Black, "PlotVWAP5U");
				AddPlot(Brushes.Black, "PlotVWAP5L");
			}
		}
		
		protected override void OnBarUpdate()
		{
			hl3 = ((High[0] + Low[0] + Close[0]) / 3);
			
			if (Bars.IsFirstBarOfSession)
			{
				iCumVolume = VOL()[0];
				iCumTypicalVolume = VOL()[0] * hl3;
				v2Sum = VOL()[0] * hl3 * hl3;
			}
			else
			{
				iCumVolume = iCumVolume + VOL()[0];
				iCumTypicalVolume = iCumTypicalVolume + ( VOL()[0] * hl3 );
				v2Sum = v2Sum + VOL()[0] * hl3 * hl3;
			}
			
			curVWAP = (iCumTypicalVolume / iCumVolume);
			deviation = Math.Sqrt(Math.Max( v2Sum/iCumVolume - curVWAP*curVWAP ,0));

			PlotVWAP[0] = curVWAP;
			
			switch(NumDeviations)
			{
				case 1:
					PlotDevOne();
					break;
				case 2:
					PlotDevTwo();
					break;
				case 3:
					PlotDevThree();
					break;	
				case 4:
					PlotDevFour();
					break;
				case 5:
					PlotDevFive();
					break;
				default:
					PlotVWAP[0] = curVWAP;
					break;
			}
			
		}
		
		//*************************************************************************************
		//
		//	Plot and draw Deviation functions
		//
		//*************************************************************************************
		
		void PlotDevOne()
		{
			PlotVWAP1U[0] = curVWAP + SD1 * deviation;
			PlotVWAP1L[0] = curVWAP - SD1 * deviation;
			Draw.Region(this, "dev1", CurrentBar, 0, PlotVWAP1U, PlotVWAP1L, null, SD1AreaBrush, SD1AreaOpacity);
		}
		
		void PlotDevTwo()
		{
			PlotDevOne();
			PlotVWAP2U[0] = curVWAP + SD2 * deviation;
			PlotVWAP2L[0] = curVWAP - SD2 * deviation;
			Draw.Region(this, "dev2", CurrentBar, 0, PlotVWAP1U, PlotVWAP2U, null, SD2AreaBrush, SD2AreaOpacity);
			Draw.Region(this, "dev3", CurrentBar, 0, PlotVWAP1L, PlotVWAP2L, null, SD2AreaBrush, SD2AreaOpacity);
		}
		
		void PlotDevThree()
		{
			PlotDevTwo();
			PlotVWAP3U[0] = curVWAP + SD3 * deviation;
			PlotVWAP3L[0] = curVWAP - SD3 * deviation;
			Draw.Region(this, "dev4", CurrentBar, 0, PlotVWAP2U, PlotVWAP3U, null, SD3AreaBrush, SD3AreaOpacity);
			Draw.Region(this, "dev5", CurrentBar, 0, PlotVWAP2L, PlotVWAP3L, null, SD3AreaBrush, SD3AreaOpacity);
		}
		
		void PlotDevFour()
		{
			PlotDevThree();
			PlotVWAP4U[0] = curVWAP + SD4 * deviation;
			PlotVWAP4L[0] = curVWAP - SD4 * deviation;
			Draw.Region(this, "dev6", CurrentBar, 0, PlotVWAP3U, PlotVWAP4U, null, SD4AreaBrush, SD4AreaOpacity);
			Draw.Region(this, "dev7", CurrentBar, 0, PlotVWAP3L, PlotVWAP4L, null, SD4AreaBrush, SD4AreaOpacity);
		}
		
		void PlotDevFive()
		{
			PlotDevFour();
			PlotVWAP5U[0] = curVWAP + SD5 * deviation;
			PlotVWAP5L[0] = curVWAP - SD5 * deviation;
			Draw.Region(this, "dev8", CurrentBar, 0, PlotVWAP4U, PlotVWAP5U, null, SD5AreaBrush, SD5AreaOpacity);
			Draw.Region(this, "dev9", CurrentBar, 0, PlotVWAP4L, PlotVWAP5L, null, SD5AreaBrush, SD5AreaOpacity);
		}

		#region Properties

		
		//*************************************************************************************
		//
		//	OPTIONS
		//
		//*************************************************************************************
		[RefreshProperties(RefreshProperties.All)]
		[Range(0, 5)]
		[Display(Name="Number of deviations", Order=1, GroupName="Standard Deviations")]
		public int NumDeviations
		{ get; set; }
		
		//Deviation 1
		[Display(Name="Deviation 1", Order=2, GroupName="Standard Deviations 1")]
		public double SD1
		{ get; set; }
		
		[Display(Name="SD1 Fill Opacity", Order=3, GroupName="Standard Deviations 1")]
		public int SD1AreaOpacity
		{ get; set; }
		
		[XmlIgnore]
		[Display(Name="SD1 Fill Color", Order=4, GroupName="Standard Deviations 1")]
		public Brush SD1AreaBrush
		{ get; set; }
		
		//Deviation 2
		[Display(Name="Deviation 2", Order=5, GroupName="Standard Deviations 2")]
		public double SD2
		{ get; set; }
				
		[Display(Name="SD2 Fill Opacity", Order=6, GroupName="Standard Deviations 2")]
		public int SD2AreaOpacity
		{ get; set; }
		
		[XmlIgnore]
		[Display(Name="SD2 Fill Color", Order=7, GroupName="Standard Deviations 2")]
		public Brush SD2AreaBrush
		{ get; set; }
		
		//Deviation 3
		[Display(Name="Deviation 3", Order=8, GroupName="Standard Deviations 3")]
		public double SD3
		{ get; set; }
		
		[Display(Name="SD3 Fill Opacity", Order=9, GroupName="Standard Deviations 3")]
		public int SD3AreaOpacity
		{ get; set; }
		
		[XmlIgnore]
		[Display(Name="SD3 Fill Color", Order=10, GroupName="Standard Deviations 3")]
		public Brush SD3AreaBrush
		{ get; set; }
		
		//Deviation 4
		[Display(Name="Deviation 4", Order=11, GroupName="Standard Deviations 4")]
		public double SD4
		{ get; set; }
		
		[Display(Name="SD4 Fill Opacity", Order=12, GroupName="Standard Deviations 4")]
		public int SD4AreaOpacity
		{ get; set; }
		
		[XmlIgnore]
		[Display(Name="SD4 Fill Color", Order=13, GroupName="Standard Deviations 4")]
		public Brush SD4AreaBrush
		{ get; set; }
		
		//Deviation 5
		[Display(Name="Deviation 5", Order=14, GroupName="Standard Deviations 5")]
		public double SD5
		{ get; set; }
		
		[Display(Name="SD5 Fill Opacity", Order=15, GroupName="Standard Deviations 5")]
		public int SD5AreaOpacity
		{ get; set; }
		
		[XmlIgnore]
		[Display(Name="SD5 Fill Color", Order=16, GroupName="Standard Deviations 5")]
		public Brush SD5AreaBrush
		{ get; set; }
		
		
		//*************************************************************************************
		//
		//	PLOT LINES AND AREAS
		//
		//*************************************************************************************
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> PlotVWAP
		{
			get { return Values[0]; }
		}
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> PlotVWAP1U
		{
			get { return Values[1]; }
		}
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> PlotVWAP1L
		{
			get { return Values[2]; }
		}
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> PlotVWAP2U
		{
			get { return Values[3]; }
		}
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> PlotVWAP2L
		{
			get { return Values[4]; }
		}
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> PlotVWAP3U
		{
			get { return Values[5]; }
		}
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> PlotVWAP3L
		{
			get { return Values[6]; }
		}
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> PlotVWAP4U
		{
			get { return Values[7]; }
		}
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> PlotVWAP4L
		{
			get { return Values[8]; }
		}
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> PlotVWAP5U
		{
			get { return Values[9]; }
		}
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> PlotVWAP5L
		{
			get { return Values[10]; }
		}
		
		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private VWAP[] cacheVWAP;
		public VWAP VWAP()
		{
			return VWAP(Input);
		}

		public VWAP VWAP(ISeries<double> input)
		{
			if (cacheVWAP != null)
				for (int idx = 0; idx < cacheVWAP.Length; idx++)
					if (cacheVWAP[idx] != null &&  cacheVWAP[idx].EqualsInput(input))
						return cacheVWAP[idx];
			return CacheIndicator<VWAP>(new VWAP(), input, ref cacheVWAP);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.VWAP VWAP()
		{
			return indicator.VWAP(Input);
		}

		public Indicators.VWAP VWAP(ISeries<double> input )
		{
			return indicator.VWAP(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.VWAP VWAP()
		{
			return indicator.VWAP(Input);
		}

		public Indicators.VWAP VWAP(ISeries<double> input )
		{
			return indicator.VWAP(input);
		}
	}
}

#endregion
