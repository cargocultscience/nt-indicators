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
using NinjaTrader.Gui.Tools;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators.Cargocult
{
	public class MultiSeriesHL : Indicator
	{
		private double my1MinLow;
		private double my5MinLow;
		private double my15MinLow;
		private double my30MinLow;
		private double my60MinLow;
		private double my240MinLow;
		private double myMondayLow;
		private double myDailyLow;
		private double myWeeklyLow;
		private double myMonthlyLow;
		
		private double my1MinHigh;
		private double my5MinHigh;
		private double my15MinHigh;
		private double my30MinHigh;
		private double my60MinHigh;
		private double my240MinHigh;
		private double myMondayHigh;
		private double myDailyHigh;
		private double myWeeklyHigh;
		private double myMonthlyHigh;
		
		private double my1MinMedian;
		private double my5MinMedian;
		private double my15MinMedian;
		private double my30MinMedian;
		private double my60MinMedian;
		private double my240MinMedian;
		private double myMondayMedian;
		private double myDailyMedian;
		private double myWeeklyMedian;
		private double myMonthlyMedian;
		

		private int barsCurrent;
		
		private int barsAtLine1;
		private int barsAtLine5;
		private int barsAtLine15;
		private int barsAtLine30;
		private int barsAtLine60;
		private int barsAtLine240;
		private int barsAtLineMonday;
		private int barsAtLineDaily;
		private int barsAtLineWeekly;
		private int barsAtLineMonthly;
		
		private CurrentDayOHL myCurrentDayOHL;
		
		private double currentOHLHigh;			
		private double currentOHLLow;
		private double currentOHLMedian;
		
		private double currentWeekMedian;
		private DateTime 				currentDate 		=	Core.Globals.MinDate;
		private double					currentWeekHigh		=	double.MinValue;
		private double					currentWeekLow		=	double.MaxValue;
		private DateTime				lastDate			= 	Core.Globals.MinDate;
		private	Data.SessionIterator	sessionIterator;
		
		private DayOfWeek				startWeekFromDay	=	DayOfWeek.Monday;
		private string version = "1.0.0.0";
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "MultiSeriesHL";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= true;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				ArePlotsConfigurable 						= false;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;
				
				
				//Levels
				min1				= false;
				min5				= false; 
				min15				= false;
				min30				= false;
				min60				= false;
				min240				= false;
				minMonthly          = false;
				minDaily			= true;
				currentDaily		= true; 
				minWeekly			= false; 
				minMonthly			= false; 
				currentWeekly		= true;
				
				//Colors
				min1Color			= Brushes.Red;
				min5Color			= Brushes.Orange; 
				min15Color			= Brushes.Blue;
				min30Color			= Brushes.Pink;
				min60Color			= Brushes.Purple;
				min240Color			= Brushes.LimeGreen;
				mondayColor			= Brushes.GreenYellow;
				minDailyColor		= Brushes.White;
				currentDailyColor   = Brushes.DarkRed;
				minWeeklyColor		= Brushes.Goldenrod;
				currentWeeklyColor	= Brushes.Tomato;
				minMonthlyColor     = Brushes.Tomato;
				
				//DashStyle
				min1Dash			= DashStyleHelper.Dash;
				min5Dash			= DashStyleHelper.Dash; 
				min15Dash			= DashStyleHelper.Dash;
				min30Dash			= DashStyleHelper.Dash;
				min60Dash			= DashStyleHelper.Dash;
				min240Dash			= DashStyleHelper.Dash;
				mondayDash			= DashStyleHelper.Dash;
				minDailyDash		= DashStyleHelper.Dash; //previous day
				currentDailyDash	= DashStyleHelper.Dash;
				minWeeklyDash		= DashStyleHelper.Dash; //previous week
				currentWeeklyDash	= DashStyleHelper.Dash;
				minMonthlyDash		= DashStyleHelper.Dash; // previous month
				
				//Labels
				min1LongLabel = "1m";
				min1ShortLabel = "1m";
				min5LongLabel = "5m";
				min5ShortLabel = "5m";
				min15LongLabel = "15m";
				min15ShortLabel = "15m";
				min30LongLabel = "30m";
				min30ShortLabel = "30m";
				min60LongLabel = "60m";
				min60ShortLabel = "60m";
				min240LongLabel = "240m";
				min240ShortLabel = "240m";
				mondayLongLabel = "Monday";
				mondayShortLabel = "Mon";
				minDailyLongLabel = "Daily";
				minDailyShortLabel = "d";
				currentDailyLongLabel = "Daily";
				currentDailyShortLabel = "d";
				minWeeklyLongLabel = "Weekly";
				minWeeklyShortLabel = "w";
				currentWeeklyLongLabel = "Weekly";
				currentWeeklyShortLabel = "w";
				minMonthlyLongLabel = "Monthly";
				minMonthlyShortLabel = "m";
				
				highLongLabel = " High";
				highShortLabel = "H";
				lowLongLabel = " Low";
				lowShortLabel = "L";
				medianLongLabel = " Mid";
				medianShortLabel = "M";
				previousLongLabel = "Prev";
				previousShortLabel = "p";
				
				//Line Width
				min1Width			= 2;
				min5Width			= 2; 
				min15Width			= 2;
				min30Width			= 2;
				min60Width			= 2;
				min240Width			= 2;
				mondayWidth			= 2;
				minDailyWidth		= 2;
				currentDailyWidth   = 2;
				minWeeklyWidth		= 2;
				currentWeeklyWidth	= 2;
				minMonthlyWidth		= 2;
				
				//Show Median Levels
				min1ShowMedian			= false;
				min5ShowMedian			= false;
				min15ShowMedian			= false;
				min30ShowMedian			= false;
				min60ShowMedian			= false;
				min240ShowMedian		= false;
				mondayShowMedian		= true;
				minDailyShowMedian		= true;
				currentDailyShowMedian	= true;
				minWeeklyShowMedian		= true;
				currentWeeklyShowMedian	= true;
				minMonthlyShowMedian	= true;
				
				//Show Labels
				min1ShowLabel			= true;
				min5ShowLabel			= true;
				min15ShowLabel			= true;
				min30ShowLabel			= true;
				min60ShowLabel			= true;
				min240ShowLabel			= true;
				mondayShowLabel		= true;
				minDailyShowLabel		= true;
				currentDailyShowLabel	= true;
				minWeeklyShowLabel		= true;
				currentWeeklyShowLabel	= true;
				minMonthlyShowLabel		= true;
		
				StartWeekFromDay		= DayOfWeek.Monday;
				useShortLabels = true;
				showLabelPrices = true;
			}
			else if (State == State.Configure)
			{
				//[0][0] = current data series
				AddDataSeries(Data.BarsPeriodType.Minute, 1); //[1][0]
				AddDataSeries(Data.BarsPeriodType.Minute, 5); //[2][0]	
				AddDataSeries(Data.BarsPeriodType.Minute, 15); //[3][0]
				AddDataSeries(Data.BarsPeriodType.Minute, 30); //[4][0]
				AddDataSeries(Data.BarsPeriodType.Minute, 60); //[5][0]
				AddDataSeries(Data.BarsPeriodType.Minute, 240); //[6][0]
				AddDataSeries(Data.BarsPeriodType.Day, 1); //[7][0]
				AddDataSeries(Data.BarsPeriodType.Week, 1); //[8][0]
				AddDataSeries(Data.BarsPeriodType.Month, 1); //[9][0]
				AddDataSeries(Data.BarsPeriodType.Day, 1); //[10][0]
				
				currentDate 	    = Core.Globals.MinDate;
				
				currentWeekHigh		= double.MinValue;
				currentWeekLow		= double.MaxValue;
				lastDate			= Core.Globals.MinDate;
				Log(string.Format("Cargocults MultiSeries High/Median/Low {0}", version), LogLevel.Information);
			}
			
			else if (State == State.DataLoaded)
			{
				ClearOutputWindow();
		
				sessionIterator = new Data.SessionIterator(Bars);		
			}
			
			
		else if (State == State.Historical)
		{
			if (!Bars.BarsType.IsIntraday)
				{
					if (currentDaily)
					{
						Draw.TextFixed(this, "CurrentDayErr", "Current Day High/Low Only works on Intraday\n", TextPosition.BottomRight);
					}
					
					if (currentWeekly)
					{
						Draw.TextFixed(this, "CurrentWeekErr", "Current Week High/Low Only works on Intraday", TextPosition.BottomRight);
					}
				}
			
			else if (State == State.Terminated)
			{
			}
		}
		
			
		}
		private string BuildLabel(string longPrefix, string shortPrefix, string longLabel, string shortLabel, string longPostfix, string shortPostfix, double price)
		{
			string label = "";
			if(useShortLabels)
			{
				label += shortPrefix + shortLabel + shortPostfix;
			} 
			else 
			{
				label += longPrefix + " " + longLabel + " " + longPostfix;
			}
			if(showLabelPrices)
			{
				label += ": " + price;
			}
			return label;
		
		}
		protected override void OnBarUpdate()
		{
			
			#region Calculate Levels
			
			if (CurrentBars[0] < 1 || CurrentBars[1] < 1 || CurrentBars[2] < 1 || CurrentBars[3] < 1 || CurrentBars[4] < 1 || CurrentBars[5] < 1 || CurrentBars[6] < 1 || CurrentBars[8] < 1)
				return;
			
			if (BarsInProgress == 0)
			{
				barsCurrent = CurrentBars[0];
				
				#region Current Daily
				
				if (currentDaily)
				{		
					currentOHLHigh 		= CurrentDayOHL().CurrentHigh[0];
					currentOHLLow 		= CurrentDayOHL().CurrentLow[0];
					
					currentOHLMedian 	= (currentOHLHigh + currentOHLLow) / 2; 
				}
				
				#endregion
				
				#region Current Weekly
				
				if (currentWeekly)
				{
					if (!Bars.BarsType.IsIntraday) 
						return;
			
					lastDate = currentDate;
					
					if (sessionIterator.GetTradingDay(Time[0]).DayOfWeek == startWeekFromDay) 	
					{
						currentDate = sessionIterator.GetTradingDay(Time[0]);
					}
					
					if (lastDate != currentDate)
					{	
						currentWeekHigh	= High[0];
						currentWeekLow	= Low[0];
					}
					
					currentWeekHigh = Math.Max(currentWeekHigh, High[0]);
					currentWeekLow = Math.Min(currentWeekLow, Low[0]);
					
					currentWeekMedian = (currentWeekHigh + currentWeekLow) / 2;
				}
				
				#endregion
			}
			
			#region 1 Min
			
			if (BarsInProgress == 1 && CurrentBars[1] > 0)
			{
				my1MinHigh 		= Highs[1][0];
				my1MinLow		= Lows[1][0];
				my1MinMedian	= Medians[1][0];
				
				barsAtLine1 = CurrentBars[0];
			}
			
			#endregion
			
			#region 5 Min
			
			if (BarsInProgress == 2 && CurrentBars[2] > 0)
			{
				my5MinHigh 		= Highs[2][0];
				my5MinLow 		= Lows[2][0];
				my5MinMedian	= Medians[2][0];
			
				barsAtLine5 = CurrentBars[0];
			}
			
			#endregion
			
			#region 15 Min
			
			if (BarsInProgress == 3 && CurrentBars[3] > 0)
			{
				my15MinHigh 	= Highs[3][0];
				my15MinLow		= Lows[3][0];
				my15MinMedian	= Medians[3][0];
			
				barsAtLine15 = CurrentBars[0];
			}
			
			#endregion
			
			#region 30 Min
			
			if (BarsInProgress == 4 && CurrentBars[4] > 0)
			{
				my30MinHigh 	= Highs[4][0];
				my30MinLow		= Lows[4][0];
				my30MinMedian	= Medians[4][0];
			
				barsAtLine30 = CurrentBars[0];
			}
			
			#endregion
			
			#region 60 Min
			
			if (BarsInProgress == 5 && CurrentBars[5] > 0)
			{
				my60MinHigh 	= Highs[5][0];
				my60MinLow 		= Lows[5][0];
				my60MinMedian	= Medians[5][0];
			
				barsAtLine60 = CurrentBars[0];
			}
			
			#endregion
			
			#region 240 Min
			
			if (BarsInProgress == 6 && CurrentBars[6] > 0)
			{
				my240MinHigh 	= Highs[6][0];
				my240MinLow 	= Lows[6][0];
				my240MinMedian	= Medians[6][0];
				
				barsAtLine240 = CurrentBars[0];
			}
			
			#endregion
			
			#region Previous Daily
			
			if (BarsInProgress == 7)
			{
				myDailyHigh		= Highs[7][0];
				myDailyLow		= Lows[7][0];
				myDailyMedian	= Medians[7][0];
				
				barsAtLineDaily = CurrentBars [0];
			}
			
			#endregion
			
			#region Previous Weekly
			
			if (BarsInProgress == 8)
			{
				myWeeklyHigh	= Highs[8][0];
				myWeeklyLow		= Lows[8][0];
				myWeeklyMedian	= Medians[8][0];
				
				barsAtLineWeekly = CurrentBars [0];
			}
			
			#endregion

			#region Previous Monthly
			
			if (BarsInProgress == 9)
			{
				myMonthlyHigh	= Highs[9][0];
				myMonthlyLow		= Lows[9][0];
				myMonthlyMedian	= Medians[9][0];
				
				barsAtLineMonthly = CurrentBars [0];
			}
			
			#endregion Previous Monthly
			
			#region Monday
			if (BarsInProgress == 10)
			{
				if(sessionIterator.GetTradingDay(Time[0]).DayOfWeek == DayOfWeek.Monday)
				{
					myMondayHigh	= Highs[10][0];
					myMondayLow		= Lows[10][0];
					myMondayMedian	= Medians[10][0];
					//Log(string.Format("in monday calc {0} {1} {2} {3}", Time[0], myMondayHigh, myMondayLow, myMondayMedian), LogLevel.Error);
					barsAtLineMonday = CurrentBars [0];
				}
			}
			
			#endregion Monday
			
			#endregion Calculate Levels
			
			#region Draw Lines
			
			NinjaTrader.Gui.Tools.SimpleFont myFont = new NinjaTrader.Gui.Tools.SimpleFont("Font", 12) { Size = 10, Bold = false };
			
			#region Previous 1
			
			if(min1)
			{
				Draw.Line(this, "1High", false, (barsCurrent - barsAtLine1), my1MinHigh, -1, my1MinHigh, min1Color, min1Dash, min1Width);
				Draw.Line(this, "1Low", false, (barsCurrent - barsAtLine1), my1MinLow, -1, my1MinLow, min1Color, min1Dash, min1Width);
				
				if (min1ShowLabel)
				{
					Draw.Text(this, "1HighText", false, BuildLabel(previousLongLabel, previousShortLabel, min1LongLabel, min1ShortLabel, highLongLabel, highShortLabel, my1MinHigh), -5, my1MinHigh, 5, min1Color, myFont, TextAlignment.Left, Brushes.Transparent, null, 1);
					Draw.Text(this, "1LowText", false, BuildLabel(previousLongLabel, previousShortLabel, min1LongLabel, min1ShortLabel, lowLongLabel, lowShortLabel, my1MinLow), -5, my1MinLow, 5, min1Color, myFont, TextAlignment.Left, Brushes.Transparent, null, 1);
				}
				
				if (min1ShowMedian)
				{
					Draw.Line(this, "1Median", false, (barsCurrent - barsAtLine1), my1MinMedian, -1, my1MinMedian, min1Color, min1Dash, min1Width);
					
					if (min1ShowLabel)
					{
						Draw.Text(this, "1MedianText", false, BuildLabel(previousLongLabel, previousShortLabel, min1LongLabel, min1ShortLabel, medianLongLabel, medianShortLabel, my1MinMedian), -5, my1MinMedian, 5, min1Color, myFont, TextAlignment.Left, Brushes.Transparent, null, 1);
					}
				}
			}
			
			#endregion
			
			#region Previous 5
			
			if (min5)
			{
				Draw.Line(this, "5High", false, (barsCurrent - barsAtLine5), my5MinHigh, -1, my5MinHigh, min5Color, min5Dash, min5Width);
				Draw.Line(this, "5Low", false, (barsCurrent - barsAtLine5), my5MinLow, -1, my5MinLow, min5Color, min5Dash, min5Width);
				
				if (min5ShowLabel)
				{
					Draw.Text(this, "5HighText", false, BuildLabel(previousLongLabel, previousShortLabel, min5LongLabel, min5ShortLabel, highLongLabel, highShortLabel, my5MinHigh), -5, my5MinHigh, 5, min5Color, myFont, TextAlignment.Left, Brushes.Transparent, null, 1);
					Draw.Text(this, "5LowText", false, BuildLabel(previousLongLabel, previousShortLabel, min5LongLabel, min5ShortLabel, lowLongLabel, lowShortLabel, my5MinLow), -5, my5MinLow, 5, min5Color, myFont, TextAlignment.Left, Brushes.Transparent, null, 1);
				}
				
				if (min5ShowMedian)
				{
					Draw.Line(this, "5Median", false, (barsCurrent - barsAtLine5), my5MinMedian, -1, my5MinMedian, min5Color, min5Dash, min5Width);
					
					if (min5ShowLabel)
					{
						Draw.Text(this, "5MedianText", false, BuildLabel(previousLongLabel, previousShortLabel, min5LongLabel, min5ShortLabel, medianLongLabel, medianShortLabel, my5MinMedian), -5, my5MinMedian, 5, min5Color, myFont, TextAlignment.Left, Brushes.Transparent, null, 1);
					}
				}
			}
			
			#endregion
			
			#region Previous 15
			
			if (min15)
			{
				Draw.Line(this, "15High", false, (barsCurrent - barsAtLine15), my15MinHigh, -1, my15MinHigh, min15Color, min15Dash, min15Width);
				Draw.Line(this, "15Low", false, (barsCurrent - barsAtLine15), my15MinLow, -1, my15MinLow, min15Color, min15Dash, min15Width);
				
				if (min15ShowLabel)
				{
					Draw.Text(this, "15HighText", false, BuildLabel(previousLongLabel, previousShortLabel, min15LongLabel, min15ShortLabel, highLongLabel, highShortLabel, my15MinHigh), -5, my15MinHigh, 5, min15Color, myFont, TextAlignment.Left, Brushes.Transparent, null, 1);
					Draw.Text(this, "15LowText", false, BuildLabel(previousLongLabel, previousShortLabel, min15LongLabel, min15ShortLabel, lowLongLabel, lowShortLabel, my15MinLow), -5, my15MinLow, 5, min15Color, myFont, TextAlignment.Left, Brushes.Transparent, null, 1);
				}
				
				if (min15ShowMedian)
				{
					Draw.Line(this, "15Median", false, (barsCurrent - barsAtLine15), my15MinMedian, -1, my15MinMedian, min15Color, min15Dash, min15Width);
					
					if (min15ShowLabel)
					{
						Draw.Text(this, "15MedianText", false, BuildLabel(previousLongLabel, previousShortLabel, min15LongLabel, min15ShortLabel, medianLongLabel, medianShortLabel, my15MinMedian), -5, my15MinMedian, 5, min15Color, myFont, TextAlignment.Left, Brushes.Transparent, null, 1);
					}
				}
			}
			
			#endregion
			
			#region Previous 30
			
			if (min30)
			{
				Draw.Line(this, "30High", false, (barsCurrent - barsAtLine30), my30MinHigh, -1, my30MinHigh, min30Color, min30Dash, min30Width);
				Draw.Line(this, "30Low", false, (barsCurrent - barsAtLine30), my30MinLow, -1, my30MinLow, min30Color, min30Dash, min30Width);
				
				if (min30ShowLabel)
				{
					Draw.Text(this, "30HighText", false, BuildLabel(previousLongLabel, previousShortLabel, min30LongLabel, min30ShortLabel, highLongLabel, highShortLabel, my30MinHigh), -5, my30MinHigh, 5, min30Color, myFont, TextAlignment.Left, Brushes.Transparent, null, 1);
					Draw.Text(this, "30LowText", false, BuildLabel(previousLongLabel, previousShortLabel, min30LongLabel, min30ShortLabel, lowLongLabel, lowShortLabel, my30MinLow), -5, my30MinLow, 5, min30Color, myFont, TextAlignment.Left, Brushes.Transparent, null, 1);
				}
				
				if (min30ShowMedian)
				{
					Draw.Line(this, "30Median", false, (barsCurrent - barsAtLine30), my30MinMedian, -1, my30MinMedian, min30Color, min30Dash, min30Width);
					
					if (min30ShowLabel)
					{
						Draw.Text(this, "30MedianText", false, BuildLabel(previousLongLabel, previousShortLabel, min30LongLabel, min30ShortLabel, medianLongLabel, medianShortLabel, my30MinMedian), -5, my30MinMedian, 5, min30Color, myFont, TextAlignment.Left, Brushes.Transparent, null, 1);
					}
				}
			}
			
			#endregion
			
			#region Previous 60
			
			if (min60)
			{
				Draw.Line(this, "60High", false, (barsCurrent - barsAtLine60), my60MinHigh, -1, my60MinHigh, min60Color, min60Dash, min60Width);
				Draw.Line(this, "60Low", false, (barsCurrent - barsAtLine60), my60MinLow, -1, my60MinLow, min60Color, min60Dash, min60Width);
				
				if (min60ShowLabel)
				{
					Draw.Text(this, "60HighText", false, BuildLabel(previousLongLabel, previousShortLabel, min60LongLabel, min60ShortLabel, highLongLabel, highShortLabel, my60MinHigh), -5, my60MinHigh, 5, min60Color, myFont, TextAlignment.Left, Brushes.Transparent, null, 1);
					Draw.Text(this, "60LowText", false, BuildLabel(previousLongLabel, previousShortLabel, min60LongLabel, min60ShortLabel, lowLongLabel, lowShortLabel, my60MinLow), -5, my60MinLow, 5, min60Color, myFont, TextAlignment.Left, Brushes.Transparent, null, 1);
				}
				if (min60ShowMedian)
				{
					Draw.Line(this, "60Median", false, (barsCurrent - barsAtLine60), my60MinMedian, -1, my60MinMedian, min60Color, min60Dash, min60Width);
					
					if (min60ShowLabel)
					{
						Draw.Text(this, "60MedianText", false, BuildLabel(previousLongLabel, previousShortLabel, min60LongLabel, min60ShortLabel, medianLongLabel, medianShortLabel, my60MinMedian), -5, my60MinMedian, 5, min60Color, myFont, TextAlignment.Left, Brushes.Transparent, null, 1);
					}
				}
			}
			
			#endregion
			
			#region Previous 240
			
			if (min240)
			{
				Draw.Line(this, "240High", false, (barsCurrent - barsAtLine240), my240MinHigh, -1, my240MinHigh, min240Color, min240Dash, min240Width);
				Draw.Line(this, "240Low", false, (barsCurrent - barsAtLine240), my240MinLow, -1, my240MinLow, min240Color, min240Dash, min240Width);
				
				if (min240ShowLabel)
				{
					Draw.Text(this, "240HighText", false, BuildLabel(previousLongLabel, previousShortLabel, min240LongLabel, min240ShortLabel, highLongLabel, highShortLabel, my240MinHigh), -5, my240MinHigh, 5, min240Color, myFont, TextAlignment.Left, Brushes.Transparent, null, 1);
					Draw.Text(this, "240LowText", false, BuildLabel(previousLongLabel, previousShortLabel, min240LongLabel, min240ShortLabel, lowLongLabel, lowShortLabel, my240MinLow), -5, my240MinLow, 5, min240Color, myFont, TextAlignment.Left, Brushes.Transparent, null, 1);
				}
				
				if (min240ShowMedian)
				{
					Draw.Line(this, "240Median", false, (barsCurrent - barsAtLine240), my240MinMedian, -1, my240MinMedian, min240Color, min240Dash, min240Width);
					
					if (min240ShowLabel)
					{
						Draw.Text(this, "240MedianText", false, BuildLabel(previousLongLabel, previousShortLabel, min240LongLabel, min240ShortLabel, medianLongLabel, medianShortLabel, my240MinMedian), -5, my240MinMedian, 5, min240Color, myFont, TextAlignment.Left, Brushes.Transparent, null, 1);
					}
				}
			}
			
			#endregion 
			
			#region Monday
			
			if (monday)
			{
				Draw.Line(this, "MondayHigh", false, (barsCurrent - barsAtLineMonday), myMondayHigh, -1, myMondayHigh, mondayColor, mondayDash, mondayWidth);
				Draw.Line(this, "MondayLow", false, (barsCurrent - barsAtLineMonday), myMondayLow, -1, myMondayLow, mondayColor, mondayDash, mondayWidth);
				
				if (mondayShowLabel)
				{
					Draw.Text(this, "MondayHighText", false, BuildLabel(previousLongLabel, previousShortLabel, mondayLongLabel, mondayShortLabel, highLongLabel, highShortLabel, myMondayHigh), -5, myMondayHigh, 5, mondayColor, myFont, TextAlignment.Left, Brushes.Transparent, null, 1);
					Draw.Text(this, "MondayLowText", false, BuildLabel(previousLongLabel, previousShortLabel, mondayLongLabel, mondayShortLabel, lowLongLabel, lowShortLabel, myMondayLow), -5, myMondayLow, 5, mondayColor, myFont, TextAlignment.Left, Brushes.Transparent, null, 1);
				}
				
				if (mondayShowMedian)
				{
					Draw.Line(this, "MondayMedian", false, (barsCurrent - barsAtLineMonday), myMondayMedian, -1, myMondayMedian, mondayColor, mondayDash, mondayWidth);
					
					if (mondayShowLabel)
					{
						Draw.Text(this, "MondayMedianText", false, BuildLabel(previousLongLabel, previousShortLabel, mondayLongLabel, mondayShortLabel, medianLongLabel, medianShortLabel, myMondayMedian), -5, myMondayMedian, 5, mondayColor, myFont, TextAlignment.Left, Brushes.Transparent, null, 1);
					}
				}
			}

			#endregion Monday
			
			#region Previous Daily
			
			if (minDaily)
			{
				Draw.Line(this, "DailyHigh", false, (barsCurrent - barsAtLineDaily), myDailyHigh, -1, myDailyHigh, minDailyColor, minDailyDash, minDailyWidth);
				Draw.Line(this, "DailyLow", false, (barsCurrent - barsAtLineDaily), myDailyLow, -1, myDailyLow, minDailyColor, minDailyDash, minDailyWidth);
				
				if (minDailyShowLabel)
				{
					Draw.Text(this, "DailyHighText", false, BuildLabel(previousLongLabel, previousShortLabel, minDailyLongLabel, minDailyShortLabel, highLongLabel, highShortLabel, myDailyHigh), -5, myDailyHigh, 5, minDailyColor, myFont, TextAlignment.Left, Brushes.Transparent, null, 1);
					Draw.Text(this, "DailyLowText", false, BuildLabel(previousLongLabel, previousShortLabel, minDailyLongLabel, minDailyShortLabel, lowLongLabel, lowShortLabel, myDailyLow), -5, myDailyLow, 5, minDailyColor, myFont, TextAlignment.Left, Brushes.Transparent, null, 1);
				}
	
				if (minDailyShowMedian)
				{
					Draw.Line(this, "DailyMedian", false, (barsCurrent - barsAtLineDaily), myDailyMedian, -1, myDailyMedian, minDailyColor, minDailyDash, minDailyWidth);
					
					if (minDailyShowLabel)
					{
						Draw.Text(this, "DailyMedianText", false, BuildLabel(previousLongLabel, previousShortLabel, minDailyLongLabel, minDailyShortLabel, medianLongLabel, medianShortLabel, myDailyMedian), -5, myDailyMedian, 5, minDailyColor, myFont, TextAlignment.Left, Brushes.Transparent, null, 1);
					}	
				}	
			}
			
			#endregion
			
			#region Current Daily
			
			if (currentDaily)
			{
				Draw.Line(this, "CurrentDailyHigh", false, (barsCurrent - barsAtLineDaily), currentOHLHigh, -1, currentOHLHigh, currentDailyColor, currentDailyDash, currentDailyWidth);
				Draw.Line(this, "CurrentDailyLow", false, (barsCurrent - barsAtLineDaily), currentOHLLow, -1, currentOHLLow, currentDailyColor, currentDailyDash, currentDailyWidth);
				
				if (currentDailyShowLabel)
				{
					Draw.Text(this, "CurrentDailyHighText", false, BuildLabel("", "", currentDailyLongLabel, currentDailyShortLabel, highLongLabel, highShortLabel, currentOHLHigh), -5, currentOHLHigh, 5, currentDailyColor, myFont, TextAlignment.Left, Brushes.Transparent, null, 1);
					Draw.Text(this, "CurrentDailyLowText", false, BuildLabel("", "", currentDailyLongLabel, currentDailyShortLabel, lowLongLabel, lowShortLabel, currentOHLLow), -5, currentOHLLow, 5, currentDailyColor, myFont, TextAlignment.Left, Brushes.Transparent, null, 1);
				}
				if (currentDailyShowMedian)
				{
					Draw.Line(this, "CurrentDailyMedian", false, (barsCurrent - barsAtLineDaily), currentOHLMedian, -1, currentOHLMedian, currentDailyColor, currentDailyDash, currentDailyWidth);
					
					if (currentDailyShowLabel)
					{
						Draw.Text(this, "CurrentDailyMedianText", false, BuildLabel("", "", currentDailyLongLabel, currentDailyShortLabel, medianLongLabel, medianShortLabel, currentOHLMedian), -5, currentOHLMedian, 5, currentDailyColor, myFont, TextAlignment.Left, Brushes.Transparent, null, 1);
					}
				}
			}
			
			#endregion Current Daily
			
			#region Previous Weekly
			
			if (minWeekly)
			{
				Draw.Line(this, "WeeklyHigh", false, (barsCurrent - barsAtLineWeekly), myWeeklyHigh, -1, myWeeklyHigh, minWeeklyColor, minWeeklyDash, minWeeklyWidth);
				Draw.Line(this, "WeeklyLow", false, (barsCurrent - barsAtLineWeekly), myWeeklyLow, -1, myWeeklyLow, minWeeklyColor, minWeeklyDash, minWeeklyWidth);
				
				if (minWeeklyShowLabel)
				{
					Draw.Text(this, "WeeklyHighText", false, BuildLabel(previousLongLabel, previousShortLabel, minWeeklyLongLabel, minWeeklyShortLabel, highLongLabel, highShortLabel, myWeeklyHigh), -5, myWeeklyHigh, 5, minWeeklyColor, myFont, TextAlignment.Left, Brushes.Transparent, null, 1);
					Draw.Text(this, "WeeklyLowText", false, BuildLabel(previousLongLabel, previousShortLabel, minWeeklyLongLabel, minWeeklyShortLabel, lowLongLabel, lowShortLabel, myWeeklyLow), -5, myWeeklyLow, 5, minWeeklyColor, myFont, TextAlignment.Left, Brushes.Transparent, null, 1);
				}
	
				if (minWeeklyShowMedian)
				{
					Draw.Line(this, "WeeklyMedian", false, (barsCurrent - barsAtLineWeekly), myWeeklyMedian, -1, myWeeklyMedian, minWeeklyColor, minWeeklyDash, minWeeklyWidth);
					
					if (minWeeklyShowLabel)
					{
						Draw.Text(this, "WeeklyMedianText", false, BuildLabel(previousLongLabel, previousShortLabel, minWeeklyLongLabel, minWeeklyShortLabel, medianLongLabel, medianShortLabel, myWeeklyMedian), -5, myWeeklyMedian, 5, minWeeklyColor, myFont, TextAlignment.Left, Brushes.Transparent, null, 1);
					}	
				}	
			}
		
			#endregion
			
			#region Current Weekly
			
			if (currentWeekly)
			{
				Draw.Line(this, "CurrentWeeklyHigh", false, (barsCurrent - barsAtLineWeekly), currentWeekHigh, -1, currentWeekHigh, currentWeeklyColor, currentWeeklyDash, currentWeeklyWidth);
				Draw.Line(this, "CurrentWeeklyLow", false, (barsCurrent - barsAtLineWeekly), currentWeekLow, -1, currentWeekLow, currentWeeklyColor, currentWeeklyDash, currentWeeklyWidth);
				
				if (currentWeeklyShowLabel)
				{
					Draw.Text(this, "CurrentWeeklyHighText", false, BuildLabel("", "", currentWeeklyLongLabel, currentWeeklyShortLabel, highLongLabel, highShortLabel, currentWeekHigh), -5, currentWeekHigh, 5, currentWeeklyColor, myFont, TextAlignment.Left, Brushes.Transparent, null, 1);
					Draw.Text(this, "CurrentWeeklyLowText", false, BuildLabel("", "", currentWeeklyLongLabel, currentWeeklyShortLabel, lowLongLabel, lowShortLabel, currentWeekLow), -5, currentWeekLow, 5, currentWeeklyColor, myFont, TextAlignment.Left, Brushes.Transparent, null, 1);
				}
				if (currentWeeklyShowMedian)
				{
					Draw.Line(this, "CurrentWeeklyMedian", false, (barsCurrent - barsAtLineWeekly), currentWeekMedian, -1, currentWeekMedian, currentWeeklyColor, currentWeeklyDash, currentWeeklyWidth);
					
					if (currentWeeklyShowLabel)
					{
						Draw.Text(this, "CurrentWeeklyMedianText", false, BuildLabel("", "", currentWeeklyLongLabel, currentWeeklyShortLabel, medianLongLabel, medianShortLabel, currentWeekMedian), -5, currentWeekMedian, 5, currentWeeklyColor, myFont, TextAlignment.Left, Brushes.Transparent, null, 1);
					}
				}
			}
			
			#endregion
			
			#region Previous Monthly
			
			if (minMonthly)
			{
				Draw.Line(this, "MonthlyHigh", false, (barsCurrent - barsAtLineMonthly), myMonthlyHigh, -1, myMonthlyHigh, minMonthlyColor, minMonthlyDash, minMonthlyWidth);
				Draw.Line(this, "MonthlyLow", false, (barsCurrent - barsAtLineMonthly), myMonthlyLow, -1, myMonthlyLow, minMonthlyColor, minMonthlyDash, minMonthlyWidth);
				
				if (minMonthlyShowLabel)
				{
					Draw.Text(this, "MonthlyHighText", false, BuildLabel(previousLongLabel, previousShortLabel, minMonthlyLongLabel, minMonthlyShortLabel, highLongLabel, highShortLabel, myMonthlyHigh), -5, myMonthlyHigh, 5, minMonthlyColor, myFont, TextAlignment.Left, Brushes.Transparent, null, 1);
					Draw.Text(this, "MonthlyLowText", false, BuildLabel(previousLongLabel, previousShortLabel, minMonthlyLongLabel, minMonthlyShortLabel, lowLongLabel, lowShortLabel, myMonthlyLow), -5, myMonthlyLow, 5, minMonthlyColor, myFont, TextAlignment.Left, Brushes.Transparent, null, 1);
				}
	
				if (minMonthlyShowMedian)
				{
					Draw.Line(this, "MonthlyMedian", false, (barsCurrent - barsAtLineMonthly), myMonthlyMedian, -1, myMonthlyMedian, minMonthlyColor, minMonthlyDash, minMonthlyWidth);
					
					if (minMonthlyShowLabel)
					{
						Draw.Text(this, "MonthlyMedianText", false, BuildLabel(previousLongLabel, previousShortLabel, minMonthlyLongLabel, minMonthlyShortLabel, medianLongLabel, medianShortLabel, myMonthlyMedian), -5, myMonthlyMedian, 5, minMonthlyColor, myFont, TextAlignment.Left, Brushes.Transparent, null, 1);
					}	
				}	
			}
			
			#endregion

			#endregion
			
		}

		#region Button Click Event
		
		private void OnButtonClick(object sender, RoutedEventArgs rea)
		{
			System.Windows.Controls.Button button = sender as System.Windows.Controls.Button;
			
			
		}
	
	#endregion
		
			
		#region Properties

		#region 00. Display Levels
		
		[NinjaScriptProperty]
		[Display(Name="1 Minute", Order=1, GroupName="00. Display Levels")]
		public bool min1
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="5 Minute", Order=2, GroupName="00. Display Levels")]
		public bool min5
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="15 Minute", Order=3, GroupName="00. Display Levels")]
		public bool min15
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="30 Minute", Order=4, GroupName="00. Display Levels")]
		public bool min30
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="60 Minute", Order=5, GroupName="00. Display Levels")]
		public bool min60
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="240 Minute/4 Hour", Order=6, GroupName="00. Display Levels")]
		public bool min240
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Monday", Order=7, GroupName="00. Display Levels")]
		public bool monday
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Previous Daily Levels", Order=8, GroupName="00. Display Levels")]
		public bool minDaily
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Current Daily Levels", Order=9, GroupName="00. Display Levels")]
		public bool currentDaily
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Previous Weekly Levels", Order=10, GroupName="00. Display Levels")]
		public bool minWeekly
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Current Weekly Levels", Order=11, GroupName="00. Display Levels")]
		public bool currentWeekly
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Previous Monthly Levels", Order=12, GroupName="00. Display Levels")]
		public bool minMonthly
		{ get; set; }

		#endregion
		
		#region 01. high/low/mid labels
		[NinjaScriptProperty]
		[Display(Name="Long High Label", Order=1, GroupName="01. Labels")]
		public string highLongLabel
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Short High Label", Order=2, GroupName="01. Labels")]
		public string highShortLabel
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Long Low Label", Order=3, GroupName="01. Labels")]
		public string lowLongLabel
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Short Low Label", Order=4, GroupName="01. Labels")]
		public string lowShortLabel
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Long Mid Label", Order=5, GroupName="01. Labels")]
		public string medianLongLabel
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Short Mid Label", Order=6, GroupName="01. Labels")]
		public string medianShortLabel
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Long Previous Label", Order=7, GroupName="01. Labels")]
		public string previousLongLabel
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Short Previous Label", Order=8, GroupName="01. Labels")]
		public string previousShortLabel
		{ get; set; }

		
		#endregion 01. high/low/mid labels
				
		#region 02. use long or short labels
		[NinjaScriptProperty]
		[Display(Name="Use Short Labels", Order=1, GroupName="02. Use Short Label")]
		public bool useShortLabels
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Use Prices In Labels", Order=2, GroupName="02. Use Short Label")]
		public bool showLabelPrices
		{ get; set; }
		
		#endregion 02. use long or short labels
		
		#region 03. 1min Level Customize
		
		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "1 Minute Color", GroupName = "03. Customize 1 Min Lines", Order = 1)]
		public Brush min1Color
		{ get; set; }

		[Browsable(false)]
		public string min1ColorSerializable
		{
			get { return Serialize.BrushToString(min1Color); }
			set { min1Color = Serialize.StringToBrush(value); }
		}			
		
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "1 Minute Dash Style", GroupName = "03. Customize 1 Min Lines", Order = 2)]
		public DashStyleHelper min1Dash
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "1 Minute Line Width", GroupName = "03. Customize 1 Min Lines", Order = 3)]
		public int min1Width
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Show 1 Min Median Levels", Order=4, GroupName="03. Customize 1 Min Lines")]
		public bool min1ShowMedian
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Show 1 Min Labels", Order=5, GroupName="03. Customize 1 Min Lines")]
		public bool min1ShowLabel
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="1 Min Long Label", Order=10, GroupName="03. Customize 1 Min Lines")]
		public string min1LongLabel
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="1 Min Short Label", Order=11, GroupName="03. Customize 1 Min Lines")]
		public string min1ShortLabel
		{ get; set; }
		
		#endregion
		
		#region 04. 5min Level Customize
		
		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "5 Minute Color", GroupName = "04. Customize 5 Min Lines", Order = 0)]
		public Brush min5Color
		{ get; set; }

		[Browsable(false)]
		public string min5ColorSerializable
		{
			get { return Serialize.BrushToString(min5Color); }
			set { min5Color = Serialize.StringToBrush(value); }
		}			
		
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "5 Minute Dash Style", GroupName = "04. Customize 5 Min Lines", Order = 2)]
		public DashStyleHelper min5Dash
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "5 Minute Line Width", GroupName = "04. Customize 5 Min Lines", Order = 3)]
		public int min5Width
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Show 5 Min Median Levels", Order=4, GroupName="04. Customize 5 Min Lines")]
		public bool min5ShowMedian
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Show 5 Min Labels", Order=5, GroupName="04. Customize 5 Min Lines")]
		public bool min5ShowLabel
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="5 Min Long Label", Order=10, GroupName="04. Customize 5 Min Lines")]
		public string min5LongLabel
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="5 Min Short Label", Order=11, GroupName="04. Customize 5 Min Lines")]
		public string min5ShortLabel
		{ get; set; }

		#endregion
		
		#region 05. 15min Level Customize
		
		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "15 Minute Color", GroupName = "05. Customize 15 Min Lines", Order = 0)]
		public Brush min15Color
		{ get; set; }

		[Browsable(false)]
		public string min15ColorSerializable
		{
			get { return Serialize.BrushToString(min15Color); }
			set { min15Color = Serialize.StringToBrush(value); }
		}			
		
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "15 Minute Dash Style", GroupName = "05. Customize 15 Min Lines", Order = 2)]
		public DashStyleHelper min15Dash
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "15 Minute Line Width", GroupName = "05. Customize 15 Min Lines", Order = 3)]
		public int min15Width
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Show 15 Min Median Levels", Order=4, GroupName="05. Customize 15 Min Lines")]
		public bool min15ShowMedian
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Show 15 Min Labels", Order=5, GroupName="05. Customize 15 Min Lines")]
		public bool min15ShowLabel
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="15 Min Long Label", Order=10, GroupName="05. Customize 15 Min Lines")]
		public string min15LongLabel
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="15 Min Short Label", Order=11, GroupName="05. Customize 15 Min Lines")]
		public string min15ShortLabel
		{ get; set; }
		
		#endregion
		
		#region 06. 30min Level Customize
		
		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "30 Minute Color", GroupName = "06. Customize 30 Min Lines", Order = 0)]
		public Brush min30Color
		{ get; set; }

		[Browsable(false)]
		public string min30ColorSerializable
		{
			get { return Serialize.BrushToString(min30Color); }
			set { min30Color = Serialize.StringToBrush(value); }
		}			
		
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "30 Minute Dash Style", GroupName = "06. Customize 30 Min Lines", Order = 2)]
		public DashStyleHelper min30Dash
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "30 Minute Line Width", GroupName = "06. Customize 30 Min Lines", Order = 3)]
		public int min30Width
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Show 30 Min Median Levels", Order=4, GroupName="06. Customize 30 Min Lines")]
		public bool min30ShowMedian
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Show 30 Min Labels", Order=5, GroupName="06. Customize 30 Min Lines")]
		public bool min30ShowLabel
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="30 Min Long Label", Order=10, GroupName="06. Customize 30 Min Lines")]
		public string min30LongLabel
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="30 Min Short Label", Order=11, GroupName="06. Customize 30 Min Lines")]
		public string min30ShortLabel
		{ get; set; }
		
		#endregion
		
		#region 07. 60min Level Customize
		
		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "60 Minute Color", GroupName = "07. Customize 60 Min Lines", Order = 0)]
		public Brush min60Color
		{ get; set; }

		[Browsable(false)]
		public string min60ColorSerializable
		{
			get { return Serialize.BrushToString(min60Color); }
			set { min60Color = Serialize.StringToBrush(value); }
		}		
		
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "60 Minute Dash Style", GroupName = "07. Customize 60 Min Lines", Order = 2)]
		public DashStyleHelper min60Dash
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "60 Minute Line Width", GroupName = "07. Customize 60 Min Lines", Order = 3)]
		public int min60Width
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Show 60 Min Median Levels", Order=4, GroupName="07. Customize 60 Min Lines")]
		public bool min60ShowMedian
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Show 60 Min Labels", Order=5, GroupName="07. Customize 60 Min Lines")]
		public bool min60ShowLabel
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="60 Min Long Label", Order=10, GroupName="07. Customize 60 Min Lines")]
		public string min60LongLabel
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="60 Min Short Label", Order=11, GroupName="07. Customize 60 Min Lines")]
		public string min60ShortLabel
		{ get; set; }
		#endregion
		
		#region 08. 240min Level Customize
		
		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "240 Minute Color", GroupName = "08. Customize 240 Min Lines", Order = 0)]
		public Brush min240Color
		{ get; set; }

		[Browsable(false)]
		public string min240ColorSerializable
		{
			get { return Serialize.BrushToString(min240Color); }
			set { min240Color = Serialize.StringToBrush(value); }
		}			
		
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "240 Minute Dash Style", GroupName = "08. Customize 240 Min Lines", Order = 2)]
		public DashStyleHelper min240Dash
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "240 Minute Line Width", GroupName = "08. Customize 240 Min Lines", Order = 3)]
		public int min240Width
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Show 240 Min Median Levels", Order=4, GroupName="08. Customize 240 Min Lines")]
		public bool min240ShowMedian
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Show 240 Min Labels", Order=5, GroupName="08. Customize 240 Min Lines")]
		public bool min240ShowLabel
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="240 Min Long Label", Order=10, GroupName="08. Customize 240 Min Lines")]
		public string min240LongLabel
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="240 Min Short Label", Order=11, GroupName="08. Customize 240 Min Lines")]
		public string min240ShortLabel
		{ get; set; }

		#endregion

		#region 09. Monday Level Customize
		
		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Monday Color", GroupName = "09. Customize Monday Lines", Order = 0)]
		public Brush mondayColor
		{ get; set; }

		[Browsable(false)]
		public string mondayColorSerializable
		{
			get { return Serialize.BrushToString(mondayColor); }
			set { mondayColor = Serialize.StringToBrush(value); }
		}			
		
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Monday Dash Style", GroupName = "09. Customize Monday Lines", Order = 2)]
		public DashStyleHelper mondayDash
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Monday Line Width", GroupName = "09. Customize Monday Lines", Order = 3)]
		public int mondayWidth
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Show Monday Median Levels", Order=4, GroupName="09. Customize Monday Lines")]
		public bool mondayShowMedian
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Show Monday Labels", Order=5, GroupName="09. Customize Monday Lines")]
		public bool mondayShowLabel
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Monday Long Label", Order=10, GroupName="09. Customize Monday Lines")]
		public string mondayLongLabel
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Monday Short Label", Order=11, GroupName="09. Customize Monday Lines")]
		public string mondayShortLabel
		{ get; set; }

		#endregion

		#region 10. Previous Daily Level Customize
		
		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Previous Daily Color", GroupName = "10. Customize Previous Daily Lines", Order = 0)]
		public Brush minDailyColor
		{ get; set; }

		[Browsable(false)]
		public string minDailyColorSerializable
		{
			get { return Serialize.BrushToString(minDailyColor); }
			set { minDailyColor = Serialize.StringToBrush(value); }
		}			
		
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Previous Daily Dash Style", GroupName = "10. Customize Previous Daily Lines", Order = 2)]
		public DashStyleHelper minDailyDash
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Previous Daily Minute Line Width", GroupName = "10. Customize Previous Daily Lines", Order = 3)]
		public int minDailyWidth
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Show Previous Daily Median Levels", Order=4, GroupName="10. Customize Previous Daily Lines")]
		public bool minDailyShowMedian
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Show Previous Daily Labels", Order=5, GroupName="10. Customize Previous Daily Lines")]
		public bool minDailyShowLabel
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Previous Daily Long Label", Order=10, GroupName="10. Customize Previous Daily Lines")]
		public string minDailyLongLabel
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Previous Daily Short Label", Order=11, GroupName="10. Customize Previous Daily Lines")]
		public string minDailyShortLabel
		{ get; set; }


		#endregion
		
		#region 11. Current Daily Level Customize
		
		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Current Daily Color", GroupName = "11. Customize Current Daily Lines", Order = 0)]
		public Brush currentDailyColor
		{ get; set; }

		[Browsable(false)]
		public string currentDailyColorSerializable
		{
			get { return Serialize.BrushToString(currentDailyColor); }
			set { currentDailyColor = Serialize.StringToBrush(value); }
		}			
		
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Current Daily Dash Style", GroupName = "11. Customize Current Daily Lines", Order = 2)]
		public DashStyleHelper currentDailyDash
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Current Daily Minute Line Width", GroupName = "11. Customize Current Daily Lines", Order = 3)]
		public int currentDailyWidth
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Show Current Daily Median Levels", Order=4, GroupName="11. Customize Current Daily Lines")]
		public bool currentDailyShowMedian
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Show Current Daily Labels", Order=5, GroupName="11. Customize Current Daily Lines")]
		public bool currentDailyShowLabel
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Current Daily Long Label", Order=10, GroupName="11. Customize Current Daily Lines")]
		public string currentDailyLongLabel
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Current Daily Short Label", Order=11, GroupName="11. Customize Current Daily Lines")]
		public string currentDailyShortLabel
		{ get; set; }

		#endregion
		
		#region 12. Previous Weekly Level Customize
		
		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Previous Weekly Color", GroupName = "12. Customize Previous Weekly Lines", Order = 0)]
		public Brush minWeeklyColor
		{ get; set; }

		[Browsable(false)]
		public string minWeeklyColorSerializable
		{
			get { return Serialize.BrushToString(minWeeklyColor); }
			set { minWeeklyColor = Serialize.StringToBrush(value); }
		}			
		
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Previous Weekly Dash Style", GroupName = "12. Customize Previous Weekly Lines", Order = 2)]
		public DashStyleHelper minWeeklyDash
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Previous Weekly Minute Line Width", GroupName = "12. Customize Previous Weekly Lines", Order = 3)]
		public int minWeeklyWidth
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Show Previous Weekly Median Levels", Order=4, GroupName="12. Customize Previous Weekly Lines")]
		public bool minWeeklyShowMedian
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Show Previous Weekly Labels", Order=5, GroupName="12. Customize Previous Weekly Lines")]
		public bool minWeeklyShowLabel
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Previous Weekly Long Label", Order=10, GroupName="12. Customize Previous Weekly Lines")]
		public string minWeeklyLongLabel
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Previous Weekly Short Label", Order=11, GroupName="12. Customize Previous Weekly Lines")]
		public string minWeeklyShortLabel
		{ get; set; }

		#endregion
		
		#region 13. Current Weekly Level Customize
		
		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Current Weekly Color", GroupName = "13. Customize Current Weekly Lines", Order = 0)]
		public Brush currentWeeklyColor
		{ get; set; }

		[Browsable(false)]
		public string currentWeeklyColorSerializable
		{
			get { return Serialize.BrushToString(currentWeeklyColor); }
			set { currentWeeklyColor = Serialize.StringToBrush(value); }
		}			
		
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Current Weekly Dash Style", GroupName = "13. Customize Current Weekly Lines", Order = 2)]
		public DashStyleHelper currentWeeklyDash
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Current Weekly Minute Line Width", GroupName = "13. Customize Current Weekly Lines", Order = 3)]
		public int currentWeeklyWidth
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Show Current Weekly Median Levels", Order=4, GroupName="13. Customize Current Weekly Lines")]
		public bool currentWeeklyShowMedian
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Show Current Weekly Labels", Order=5, GroupName="13. Customize Current Weekly Lines")]
		public bool currentWeeklyShowLabel
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Start Week Day: ", Order=6, GroupName="13. Customize Current Weekly Lines")]
		public DayOfWeek StartWeekFromDay
		{
			get { return startWeekFromDay; }
			set { startWeekFromDay = value; }
		}
		
		[NinjaScriptProperty]
		[Display(Name="Previous Weekly Long Label", Order=10, GroupName="13. Customize Current Weekly Lines")]
		public string currentWeeklyLongLabel
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Previous Weekly Short Label", Order=11, GroupName="13. Customize Current Weekly Lines")]
		public string currentWeeklyShortLabel
		{ get; set; }
		
		#endregion
		
		#region 14. Previous Monthly Level Customize
		
		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Previous Monthly Color", GroupName = "14. Customize Previous Monthly Lines", Order = 0)]
		public Brush minMonthlyColor
		{ get; set; }

		[Browsable(false)]
		public string minMonthlyColorSerializable
		{
			get { return Serialize.BrushToString(minMonthlyColor); }
			set { minMonthlyColor = Serialize.StringToBrush(value); }
		}			
		
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Previous Monthly Dash Style", GroupName = "14. Customize Previous Monthly Lines", Order = 2)]
		public DashStyleHelper minMonthlyDash
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Previous Monthly Minute Line Width", GroupName = "14. Customize Previous Monthly Lines", Order = 3)]
		public int minMonthlyWidth
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Show Previous Monthly Median Levels", Order=4, GroupName="14. Customize Previous Monthly Lines")]
		public bool minMonthlyShowMedian
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Show Previous Monthly Labels", Order=5, GroupName="14. Customize Previous Monthly Lines")]
		public bool minMonthlyShowLabel
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Previous Monthly Long Label", Order=10, GroupName="14. Customize Previous Monthly Lines")]
		public string minMonthlyLongLabel
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Previous Monthly Short Label", Order=11, GroupName="14. Customize Previous Monthly Lines")]
		public string minMonthlyShortLabel
		{ get; set; }

		#endregion
		
		#endregion
		
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private Cargocult.MultiSeriesHL[] cacheMultiSeriesHL;
		public Cargocult.MultiSeriesHL MultiSeriesHL(bool min1, bool min5, bool min15, bool min30, bool min60, bool min240, bool monday, bool minDaily, bool currentDaily, bool minWeekly, bool currentWeekly, bool minMonthly, string highLongLabel, string highShortLabel, string lowLongLabel, string lowShortLabel, string medianLongLabel, string medianShortLabel, string previousLongLabel, string previousShortLabel, bool useShortLabels, bool showLabelPrices, Brush min1Color, DashStyleHelper min1Dash, int min1Width, bool min1ShowMedian, bool min1ShowLabel, string min1LongLabel, string min1ShortLabel, Brush min5Color, DashStyleHelper min5Dash, int min5Width, bool min5ShowMedian, bool min5ShowLabel, string min5LongLabel, string min5ShortLabel, Brush min15Color, DashStyleHelper min15Dash, int min15Width, bool min15ShowMedian, bool min15ShowLabel, string min15LongLabel, string min15ShortLabel, Brush min30Color, DashStyleHelper min30Dash, int min30Width, bool min30ShowMedian, bool min30ShowLabel, string min30LongLabel, string min30ShortLabel, Brush min60Color, DashStyleHelper min60Dash, int min60Width, bool min60ShowMedian, bool min60ShowLabel, string min60LongLabel, string min60ShortLabel, Brush min240Color, DashStyleHelper min240Dash, int min240Width, bool min240ShowMedian, bool min240ShowLabel, string min240LongLabel, string min240ShortLabel, Brush mondayColor, DashStyleHelper mondayDash, int mondayWidth, bool mondayShowMedian, bool mondayShowLabel, string mondayLongLabel, string mondayShortLabel, Brush minDailyColor, DashStyleHelper minDailyDash, int minDailyWidth, bool minDailyShowMedian, bool minDailyShowLabel, string minDailyLongLabel, string minDailyShortLabel, Brush currentDailyColor, DashStyleHelper currentDailyDash, int currentDailyWidth, bool currentDailyShowMedian, bool currentDailyShowLabel, string currentDailyLongLabel, string currentDailyShortLabel, Brush minWeeklyColor, DashStyleHelper minWeeklyDash, int minWeeklyWidth, bool minWeeklyShowMedian, bool minWeeklyShowLabel, string minWeeklyLongLabel, string minWeeklyShortLabel, Brush currentWeeklyColor, DashStyleHelper currentWeeklyDash, int currentWeeklyWidth, bool currentWeeklyShowMedian, bool currentWeeklyShowLabel, DayOfWeek startWeekFromDay, string currentWeeklyLongLabel, string currentWeeklyShortLabel, Brush minMonthlyColor, DashStyleHelper minMonthlyDash, int minMonthlyWidth, bool minMonthlyShowMedian, bool minMonthlyShowLabel, string minMonthlyLongLabel, string minMonthlyShortLabel)
		{
			return MultiSeriesHL(Input, min1, min5, min15, min30, min60, min240, monday, minDaily, currentDaily, minWeekly, currentWeekly, minMonthly, highLongLabel, highShortLabel, lowLongLabel, lowShortLabel, medianLongLabel, medianShortLabel, previousLongLabel, previousShortLabel, useShortLabels, showLabelPrices, min1Color, min1Dash, min1Width, min1ShowMedian, min1ShowLabel, min1LongLabel, min1ShortLabel, min5Color, min5Dash, min5Width, min5ShowMedian, min5ShowLabel, min5LongLabel, min5ShortLabel, min15Color, min15Dash, min15Width, min15ShowMedian, min15ShowLabel, min15LongLabel, min15ShortLabel, min30Color, min30Dash, min30Width, min30ShowMedian, min30ShowLabel, min30LongLabel, min30ShortLabel, min60Color, min60Dash, min60Width, min60ShowMedian, min60ShowLabel, min60LongLabel, min60ShortLabel, min240Color, min240Dash, min240Width, min240ShowMedian, min240ShowLabel, min240LongLabel, min240ShortLabel, mondayColor, mondayDash, mondayWidth, mondayShowMedian, mondayShowLabel, mondayLongLabel, mondayShortLabel, minDailyColor, minDailyDash, minDailyWidth, minDailyShowMedian, minDailyShowLabel, minDailyLongLabel, minDailyShortLabel, currentDailyColor, currentDailyDash, currentDailyWidth, currentDailyShowMedian, currentDailyShowLabel, currentDailyLongLabel, currentDailyShortLabel, minWeeklyColor, minWeeklyDash, minWeeklyWidth, minWeeklyShowMedian, minWeeklyShowLabel, minWeeklyLongLabel, minWeeklyShortLabel, currentWeeklyColor, currentWeeklyDash, currentWeeklyWidth, currentWeeklyShowMedian, currentWeeklyShowLabel, startWeekFromDay, currentWeeklyLongLabel, currentWeeklyShortLabel, minMonthlyColor, minMonthlyDash, minMonthlyWidth, minMonthlyShowMedian, minMonthlyShowLabel, minMonthlyLongLabel, minMonthlyShortLabel);
		}

		public Cargocult.MultiSeriesHL MultiSeriesHL(ISeries<double> input, bool min1, bool min5, bool min15, bool min30, bool min60, bool min240, bool monday, bool minDaily, bool currentDaily, bool minWeekly, bool currentWeekly, bool minMonthly, string highLongLabel, string highShortLabel, string lowLongLabel, string lowShortLabel, string medianLongLabel, string medianShortLabel, string previousLongLabel, string previousShortLabel, bool useShortLabels, bool showLabelPrices, Brush min1Color, DashStyleHelper min1Dash, int min1Width, bool min1ShowMedian, bool min1ShowLabel, string min1LongLabel, string min1ShortLabel, Brush min5Color, DashStyleHelper min5Dash, int min5Width, bool min5ShowMedian, bool min5ShowLabel, string min5LongLabel, string min5ShortLabel, Brush min15Color, DashStyleHelper min15Dash, int min15Width, bool min15ShowMedian, bool min15ShowLabel, string min15LongLabel, string min15ShortLabel, Brush min30Color, DashStyleHelper min30Dash, int min30Width, bool min30ShowMedian, bool min30ShowLabel, string min30LongLabel, string min30ShortLabel, Brush min60Color, DashStyleHelper min60Dash, int min60Width, bool min60ShowMedian, bool min60ShowLabel, string min60LongLabel, string min60ShortLabel, Brush min240Color, DashStyleHelper min240Dash, int min240Width, bool min240ShowMedian, bool min240ShowLabel, string min240LongLabel, string min240ShortLabel, Brush mondayColor, DashStyleHelper mondayDash, int mondayWidth, bool mondayShowMedian, bool mondayShowLabel, string mondayLongLabel, string mondayShortLabel, Brush minDailyColor, DashStyleHelper minDailyDash, int minDailyWidth, bool minDailyShowMedian, bool minDailyShowLabel, string minDailyLongLabel, string minDailyShortLabel, Brush currentDailyColor, DashStyleHelper currentDailyDash, int currentDailyWidth, bool currentDailyShowMedian, bool currentDailyShowLabel, string currentDailyLongLabel, string currentDailyShortLabel, Brush minWeeklyColor, DashStyleHelper minWeeklyDash, int minWeeklyWidth, bool minWeeklyShowMedian, bool minWeeklyShowLabel, string minWeeklyLongLabel, string minWeeklyShortLabel, Brush currentWeeklyColor, DashStyleHelper currentWeeklyDash, int currentWeeklyWidth, bool currentWeeklyShowMedian, bool currentWeeklyShowLabel, DayOfWeek startWeekFromDay, string currentWeeklyLongLabel, string currentWeeklyShortLabel, Brush minMonthlyColor, DashStyleHelper minMonthlyDash, int minMonthlyWidth, bool minMonthlyShowMedian, bool minMonthlyShowLabel, string minMonthlyLongLabel, string minMonthlyShortLabel)
		{
			if (cacheMultiSeriesHL != null)
				for (int idx = 0; idx < cacheMultiSeriesHL.Length; idx++)
					if (cacheMultiSeriesHL[idx] != null && cacheMultiSeriesHL[idx].min1 == min1 && cacheMultiSeriesHL[idx].min5 == min5 && cacheMultiSeriesHL[idx].min15 == min15 && cacheMultiSeriesHL[idx].min30 == min30 && cacheMultiSeriesHL[idx].min60 == min60 && cacheMultiSeriesHL[idx].min240 == min240 && cacheMultiSeriesHL[idx].monday == monday && cacheMultiSeriesHL[idx].minDaily == minDaily && cacheMultiSeriesHL[idx].currentDaily == currentDaily && cacheMultiSeriesHL[idx].minWeekly == minWeekly && cacheMultiSeriesHL[idx].currentWeekly == currentWeekly && cacheMultiSeriesHL[idx].minMonthly == minMonthly && cacheMultiSeriesHL[idx].highLongLabel == highLongLabel && cacheMultiSeriesHL[idx].highShortLabel == highShortLabel && cacheMultiSeriesHL[idx].lowLongLabel == lowLongLabel && cacheMultiSeriesHL[idx].lowShortLabel == lowShortLabel && cacheMultiSeriesHL[idx].medianLongLabel == medianLongLabel && cacheMultiSeriesHL[idx].medianShortLabel == medianShortLabel && cacheMultiSeriesHL[idx].previousLongLabel == previousLongLabel && cacheMultiSeriesHL[idx].previousShortLabel == previousShortLabel && cacheMultiSeriesHL[idx].useShortLabels == useShortLabels && cacheMultiSeriesHL[idx].showLabelPrices == showLabelPrices && cacheMultiSeriesHL[idx].min1Color == min1Color && cacheMultiSeriesHL[idx].min1Dash == min1Dash && cacheMultiSeriesHL[idx].min1Width == min1Width && cacheMultiSeriesHL[idx].min1ShowMedian == min1ShowMedian && cacheMultiSeriesHL[idx].min1ShowLabel == min1ShowLabel && cacheMultiSeriesHL[idx].min1LongLabel == min1LongLabel && cacheMultiSeriesHL[idx].min1ShortLabel == min1ShortLabel && cacheMultiSeriesHL[idx].min5Color == min5Color && cacheMultiSeriesHL[idx].min5Dash == min5Dash && cacheMultiSeriesHL[idx].min5Width == min5Width && cacheMultiSeriesHL[idx].min5ShowMedian == min5ShowMedian && cacheMultiSeriesHL[idx].min5ShowLabel == min5ShowLabel && cacheMultiSeriesHL[idx].min5LongLabel == min5LongLabel && cacheMultiSeriesHL[idx].min5ShortLabel == min5ShortLabel && cacheMultiSeriesHL[idx].min15Color == min15Color && cacheMultiSeriesHL[idx].min15Dash == min15Dash && cacheMultiSeriesHL[idx].min15Width == min15Width && cacheMultiSeriesHL[idx].min15ShowMedian == min15ShowMedian && cacheMultiSeriesHL[idx].min15ShowLabel == min15ShowLabel && cacheMultiSeriesHL[idx].min15LongLabel == min15LongLabel && cacheMultiSeriesHL[idx].min15ShortLabel == min15ShortLabel && cacheMultiSeriesHL[idx].min30Color == min30Color && cacheMultiSeriesHL[idx].min30Dash == min30Dash && cacheMultiSeriesHL[idx].min30Width == min30Width && cacheMultiSeriesHL[idx].min30ShowMedian == min30ShowMedian && cacheMultiSeriesHL[idx].min30ShowLabel == min30ShowLabel && cacheMultiSeriesHL[idx].min30LongLabel == min30LongLabel && cacheMultiSeriesHL[idx].min30ShortLabel == min30ShortLabel && cacheMultiSeriesHL[idx].min60Color == min60Color && cacheMultiSeriesHL[idx].min60Dash == min60Dash && cacheMultiSeriesHL[idx].min60Width == min60Width && cacheMultiSeriesHL[idx].min60ShowMedian == min60ShowMedian && cacheMultiSeriesHL[idx].min60ShowLabel == min60ShowLabel && cacheMultiSeriesHL[idx].min60LongLabel == min60LongLabel && cacheMultiSeriesHL[idx].min60ShortLabel == min60ShortLabel && cacheMultiSeriesHL[idx].min240Color == min240Color && cacheMultiSeriesHL[idx].min240Dash == min240Dash && cacheMultiSeriesHL[idx].min240Width == min240Width && cacheMultiSeriesHL[idx].min240ShowMedian == min240ShowMedian && cacheMultiSeriesHL[idx].min240ShowLabel == min240ShowLabel && cacheMultiSeriesHL[idx].min240LongLabel == min240LongLabel && cacheMultiSeriesHL[idx].min240ShortLabel == min240ShortLabel && cacheMultiSeriesHL[idx].mondayColor == mondayColor && cacheMultiSeriesHL[idx].mondayDash == mondayDash && cacheMultiSeriesHL[idx].mondayWidth == mondayWidth && cacheMultiSeriesHL[idx].mondayShowMedian == mondayShowMedian && cacheMultiSeriesHL[idx].mondayShowLabel == mondayShowLabel && cacheMultiSeriesHL[idx].mondayLongLabel == mondayLongLabel && cacheMultiSeriesHL[idx].mondayShortLabel == mondayShortLabel && cacheMultiSeriesHL[idx].minDailyColor == minDailyColor && cacheMultiSeriesHL[idx].minDailyDash == minDailyDash && cacheMultiSeriesHL[idx].minDailyWidth == minDailyWidth && cacheMultiSeriesHL[idx].minDailyShowMedian == minDailyShowMedian && cacheMultiSeriesHL[idx].minDailyShowLabel == minDailyShowLabel && cacheMultiSeriesHL[idx].minDailyLongLabel == minDailyLongLabel && cacheMultiSeriesHL[idx].minDailyShortLabel == minDailyShortLabel && cacheMultiSeriesHL[idx].currentDailyColor == currentDailyColor && cacheMultiSeriesHL[idx].currentDailyDash == currentDailyDash && cacheMultiSeriesHL[idx].currentDailyWidth == currentDailyWidth && cacheMultiSeriesHL[idx].currentDailyShowMedian == currentDailyShowMedian && cacheMultiSeriesHL[idx].currentDailyShowLabel == currentDailyShowLabel && cacheMultiSeriesHL[idx].currentDailyLongLabel == currentDailyLongLabel && cacheMultiSeriesHL[idx].currentDailyShortLabel == currentDailyShortLabel && cacheMultiSeriesHL[idx].minWeeklyColor == minWeeklyColor && cacheMultiSeriesHL[idx].minWeeklyDash == minWeeklyDash && cacheMultiSeriesHL[idx].minWeeklyWidth == minWeeklyWidth && cacheMultiSeriesHL[idx].minWeeklyShowMedian == minWeeklyShowMedian && cacheMultiSeriesHL[idx].minWeeklyShowLabel == minWeeklyShowLabel && cacheMultiSeriesHL[idx].minWeeklyLongLabel == minWeeklyLongLabel && cacheMultiSeriesHL[idx].minWeeklyShortLabel == minWeeklyShortLabel && cacheMultiSeriesHL[idx].currentWeeklyColor == currentWeeklyColor && cacheMultiSeriesHL[idx].currentWeeklyDash == currentWeeklyDash && cacheMultiSeriesHL[idx].currentWeeklyWidth == currentWeeklyWidth && cacheMultiSeriesHL[idx].currentWeeklyShowMedian == currentWeeklyShowMedian && cacheMultiSeriesHL[idx].currentWeeklyShowLabel == currentWeeklyShowLabel && cacheMultiSeriesHL[idx].StartWeekFromDay == startWeekFromDay && cacheMultiSeriesHL[idx].currentWeeklyLongLabel == currentWeeklyLongLabel && cacheMultiSeriesHL[idx].currentWeeklyShortLabel == currentWeeklyShortLabel && cacheMultiSeriesHL[idx].minMonthlyColor == minMonthlyColor && cacheMultiSeriesHL[idx].minMonthlyDash == minMonthlyDash && cacheMultiSeriesHL[idx].minMonthlyWidth == minMonthlyWidth && cacheMultiSeriesHL[idx].minMonthlyShowMedian == minMonthlyShowMedian && cacheMultiSeriesHL[idx].minMonthlyShowLabel == minMonthlyShowLabel && cacheMultiSeriesHL[idx].minMonthlyLongLabel == minMonthlyLongLabel && cacheMultiSeriesHL[idx].minMonthlyShortLabel == minMonthlyShortLabel && cacheMultiSeriesHL[idx].EqualsInput(input))
						return cacheMultiSeriesHL[idx];
			return CacheIndicator<Cargocult.MultiSeriesHL>(new Cargocult.MultiSeriesHL(){ min1 = min1, min5 = min5, min15 = min15, min30 = min30, min60 = min60, min240 = min240, monday = monday, minDaily = minDaily, currentDaily = currentDaily, minWeekly = minWeekly, currentWeekly = currentWeekly, minMonthly = minMonthly, highLongLabel = highLongLabel, highShortLabel = highShortLabel, lowLongLabel = lowLongLabel, lowShortLabel = lowShortLabel, medianLongLabel = medianLongLabel, medianShortLabel = medianShortLabel, previousLongLabel = previousLongLabel, previousShortLabel = previousShortLabel, useShortLabels = useShortLabels, showLabelPrices = showLabelPrices, min1Color = min1Color, min1Dash = min1Dash, min1Width = min1Width, min1ShowMedian = min1ShowMedian, min1ShowLabel = min1ShowLabel, min1LongLabel = min1LongLabel, min1ShortLabel = min1ShortLabel, min5Color = min5Color, min5Dash = min5Dash, min5Width = min5Width, min5ShowMedian = min5ShowMedian, min5ShowLabel = min5ShowLabel, min5LongLabel = min5LongLabel, min5ShortLabel = min5ShortLabel, min15Color = min15Color, min15Dash = min15Dash, min15Width = min15Width, min15ShowMedian = min15ShowMedian, min15ShowLabel = min15ShowLabel, min15LongLabel = min15LongLabel, min15ShortLabel = min15ShortLabel, min30Color = min30Color, min30Dash = min30Dash, min30Width = min30Width, min30ShowMedian = min30ShowMedian, min30ShowLabel = min30ShowLabel, min30LongLabel = min30LongLabel, min30ShortLabel = min30ShortLabel, min60Color = min60Color, min60Dash = min60Dash, min60Width = min60Width, min60ShowMedian = min60ShowMedian, min60ShowLabel = min60ShowLabel, min60LongLabel = min60LongLabel, min60ShortLabel = min60ShortLabel, min240Color = min240Color, min240Dash = min240Dash, min240Width = min240Width, min240ShowMedian = min240ShowMedian, min240ShowLabel = min240ShowLabel, min240LongLabel = min240LongLabel, min240ShortLabel = min240ShortLabel, mondayColor = mondayColor, mondayDash = mondayDash, mondayWidth = mondayWidth, mondayShowMedian = mondayShowMedian, mondayShowLabel = mondayShowLabel, mondayLongLabel = mondayLongLabel, mondayShortLabel = mondayShortLabel, minDailyColor = minDailyColor, minDailyDash = minDailyDash, minDailyWidth = minDailyWidth, minDailyShowMedian = minDailyShowMedian, minDailyShowLabel = minDailyShowLabel, minDailyLongLabel = minDailyLongLabel, minDailyShortLabel = minDailyShortLabel, currentDailyColor = currentDailyColor, currentDailyDash = currentDailyDash, currentDailyWidth = currentDailyWidth, currentDailyShowMedian = currentDailyShowMedian, currentDailyShowLabel = currentDailyShowLabel, currentDailyLongLabel = currentDailyLongLabel, currentDailyShortLabel = currentDailyShortLabel, minWeeklyColor = minWeeklyColor, minWeeklyDash = minWeeklyDash, minWeeklyWidth = minWeeklyWidth, minWeeklyShowMedian = minWeeklyShowMedian, minWeeklyShowLabel = minWeeklyShowLabel, minWeeklyLongLabel = minWeeklyLongLabel, minWeeklyShortLabel = minWeeklyShortLabel, currentWeeklyColor = currentWeeklyColor, currentWeeklyDash = currentWeeklyDash, currentWeeklyWidth = currentWeeklyWidth, currentWeeklyShowMedian = currentWeeklyShowMedian, currentWeeklyShowLabel = currentWeeklyShowLabel, StartWeekFromDay = startWeekFromDay, currentWeeklyLongLabel = currentWeeklyLongLabel, currentWeeklyShortLabel = currentWeeklyShortLabel, minMonthlyColor = minMonthlyColor, minMonthlyDash = minMonthlyDash, minMonthlyWidth = minMonthlyWidth, minMonthlyShowMedian = minMonthlyShowMedian, minMonthlyShowLabel = minMonthlyShowLabel, minMonthlyLongLabel = minMonthlyLongLabel, minMonthlyShortLabel = minMonthlyShortLabel }, input, ref cacheMultiSeriesHL);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Cargocult.MultiSeriesHL MultiSeriesHL(bool min1, bool min5, bool min15, bool min30, bool min60, bool min240, bool monday, bool minDaily, bool currentDaily, bool minWeekly, bool currentWeekly, bool minMonthly, string highLongLabel, string highShortLabel, string lowLongLabel, string lowShortLabel, string medianLongLabel, string medianShortLabel, string previousLongLabel, string previousShortLabel, bool useShortLabels, bool showLabelPrices, Brush min1Color, DashStyleHelper min1Dash, int min1Width, bool min1ShowMedian, bool min1ShowLabel, string min1LongLabel, string min1ShortLabel, Brush min5Color, DashStyleHelper min5Dash, int min5Width, bool min5ShowMedian, bool min5ShowLabel, string min5LongLabel, string min5ShortLabel, Brush min15Color, DashStyleHelper min15Dash, int min15Width, bool min15ShowMedian, bool min15ShowLabel, string min15LongLabel, string min15ShortLabel, Brush min30Color, DashStyleHelper min30Dash, int min30Width, bool min30ShowMedian, bool min30ShowLabel, string min30LongLabel, string min30ShortLabel, Brush min60Color, DashStyleHelper min60Dash, int min60Width, bool min60ShowMedian, bool min60ShowLabel, string min60LongLabel, string min60ShortLabel, Brush min240Color, DashStyleHelper min240Dash, int min240Width, bool min240ShowMedian, bool min240ShowLabel, string min240LongLabel, string min240ShortLabel, Brush mondayColor, DashStyleHelper mondayDash, int mondayWidth, bool mondayShowMedian, bool mondayShowLabel, string mondayLongLabel, string mondayShortLabel, Brush minDailyColor, DashStyleHelper minDailyDash, int minDailyWidth, bool minDailyShowMedian, bool minDailyShowLabel, string minDailyLongLabel, string minDailyShortLabel, Brush currentDailyColor, DashStyleHelper currentDailyDash, int currentDailyWidth, bool currentDailyShowMedian, bool currentDailyShowLabel, string currentDailyLongLabel, string currentDailyShortLabel, Brush minWeeklyColor, DashStyleHelper minWeeklyDash, int minWeeklyWidth, bool minWeeklyShowMedian, bool minWeeklyShowLabel, string minWeeklyLongLabel, string minWeeklyShortLabel, Brush currentWeeklyColor, DashStyleHelper currentWeeklyDash, int currentWeeklyWidth, bool currentWeeklyShowMedian, bool currentWeeklyShowLabel, DayOfWeek startWeekFromDay, string currentWeeklyLongLabel, string currentWeeklyShortLabel, Brush minMonthlyColor, DashStyleHelper minMonthlyDash, int minMonthlyWidth, bool minMonthlyShowMedian, bool minMonthlyShowLabel, string minMonthlyLongLabel, string minMonthlyShortLabel)
		{
			return indicator.MultiSeriesHL(Input, min1, min5, min15, min30, min60, min240, monday, minDaily, currentDaily, minWeekly, currentWeekly, minMonthly, highLongLabel, highShortLabel, lowLongLabel, lowShortLabel, medianLongLabel, medianShortLabel, previousLongLabel, previousShortLabel, useShortLabels, showLabelPrices, min1Color, min1Dash, min1Width, min1ShowMedian, min1ShowLabel, min1LongLabel, min1ShortLabel, min5Color, min5Dash, min5Width, min5ShowMedian, min5ShowLabel, min5LongLabel, min5ShortLabel, min15Color, min15Dash, min15Width, min15ShowMedian, min15ShowLabel, min15LongLabel, min15ShortLabel, min30Color, min30Dash, min30Width, min30ShowMedian, min30ShowLabel, min30LongLabel, min30ShortLabel, min60Color, min60Dash, min60Width, min60ShowMedian, min60ShowLabel, min60LongLabel, min60ShortLabel, min240Color, min240Dash, min240Width, min240ShowMedian, min240ShowLabel, min240LongLabel, min240ShortLabel, mondayColor, mondayDash, mondayWidth, mondayShowMedian, mondayShowLabel, mondayLongLabel, mondayShortLabel, minDailyColor, minDailyDash, minDailyWidth, minDailyShowMedian, minDailyShowLabel, minDailyLongLabel, minDailyShortLabel, currentDailyColor, currentDailyDash, currentDailyWidth, currentDailyShowMedian, currentDailyShowLabel, currentDailyLongLabel, currentDailyShortLabel, minWeeklyColor, minWeeklyDash, minWeeklyWidth, minWeeklyShowMedian, minWeeklyShowLabel, minWeeklyLongLabel, minWeeklyShortLabel, currentWeeklyColor, currentWeeklyDash, currentWeeklyWidth, currentWeeklyShowMedian, currentWeeklyShowLabel, startWeekFromDay, currentWeeklyLongLabel, currentWeeklyShortLabel, minMonthlyColor, minMonthlyDash, minMonthlyWidth, minMonthlyShowMedian, minMonthlyShowLabel, minMonthlyLongLabel, minMonthlyShortLabel);
		}

		public Indicators.Cargocult.MultiSeriesHL MultiSeriesHL(ISeries<double> input , bool min1, bool min5, bool min15, bool min30, bool min60, bool min240, bool monday, bool minDaily, bool currentDaily, bool minWeekly, bool currentWeekly, bool minMonthly, string highLongLabel, string highShortLabel, string lowLongLabel, string lowShortLabel, string medianLongLabel, string medianShortLabel, string previousLongLabel, string previousShortLabel, bool useShortLabels, bool showLabelPrices, Brush min1Color, DashStyleHelper min1Dash, int min1Width, bool min1ShowMedian, bool min1ShowLabel, string min1LongLabel, string min1ShortLabel, Brush min5Color, DashStyleHelper min5Dash, int min5Width, bool min5ShowMedian, bool min5ShowLabel, string min5LongLabel, string min5ShortLabel, Brush min15Color, DashStyleHelper min15Dash, int min15Width, bool min15ShowMedian, bool min15ShowLabel, string min15LongLabel, string min15ShortLabel, Brush min30Color, DashStyleHelper min30Dash, int min30Width, bool min30ShowMedian, bool min30ShowLabel, string min30LongLabel, string min30ShortLabel, Brush min60Color, DashStyleHelper min60Dash, int min60Width, bool min60ShowMedian, bool min60ShowLabel, string min60LongLabel, string min60ShortLabel, Brush min240Color, DashStyleHelper min240Dash, int min240Width, bool min240ShowMedian, bool min240ShowLabel, string min240LongLabel, string min240ShortLabel, Brush mondayColor, DashStyleHelper mondayDash, int mondayWidth, bool mondayShowMedian, bool mondayShowLabel, string mondayLongLabel, string mondayShortLabel, Brush minDailyColor, DashStyleHelper minDailyDash, int minDailyWidth, bool minDailyShowMedian, bool minDailyShowLabel, string minDailyLongLabel, string minDailyShortLabel, Brush currentDailyColor, DashStyleHelper currentDailyDash, int currentDailyWidth, bool currentDailyShowMedian, bool currentDailyShowLabel, string currentDailyLongLabel, string currentDailyShortLabel, Brush minWeeklyColor, DashStyleHelper minWeeklyDash, int minWeeklyWidth, bool minWeeklyShowMedian, bool minWeeklyShowLabel, string minWeeklyLongLabel, string minWeeklyShortLabel, Brush currentWeeklyColor, DashStyleHelper currentWeeklyDash, int currentWeeklyWidth, bool currentWeeklyShowMedian, bool currentWeeklyShowLabel, DayOfWeek startWeekFromDay, string currentWeeklyLongLabel, string currentWeeklyShortLabel, Brush minMonthlyColor, DashStyleHelper minMonthlyDash, int minMonthlyWidth, bool minMonthlyShowMedian, bool minMonthlyShowLabel, string minMonthlyLongLabel, string minMonthlyShortLabel)
		{
			return indicator.MultiSeriesHL(input, min1, min5, min15, min30, min60, min240, monday, minDaily, currentDaily, minWeekly, currentWeekly, minMonthly, highLongLabel, highShortLabel, lowLongLabel, lowShortLabel, medianLongLabel, medianShortLabel, previousLongLabel, previousShortLabel, useShortLabels, showLabelPrices, min1Color, min1Dash, min1Width, min1ShowMedian, min1ShowLabel, min1LongLabel, min1ShortLabel, min5Color, min5Dash, min5Width, min5ShowMedian, min5ShowLabel, min5LongLabel, min5ShortLabel, min15Color, min15Dash, min15Width, min15ShowMedian, min15ShowLabel, min15LongLabel, min15ShortLabel, min30Color, min30Dash, min30Width, min30ShowMedian, min30ShowLabel, min30LongLabel, min30ShortLabel, min60Color, min60Dash, min60Width, min60ShowMedian, min60ShowLabel, min60LongLabel, min60ShortLabel, min240Color, min240Dash, min240Width, min240ShowMedian, min240ShowLabel, min240LongLabel, min240ShortLabel, mondayColor, mondayDash, mondayWidth, mondayShowMedian, mondayShowLabel, mondayLongLabel, mondayShortLabel, minDailyColor, minDailyDash, minDailyWidth, minDailyShowMedian, minDailyShowLabel, minDailyLongLabel, minDailyShortLabel, currentDailyColor, currentDailyDash, currentDailyWidth, currentDailyShowMedian, currentDailyShowLabel, currentDailyLongLabel, currentDailyShortLabel, minWeeklyColor, minWeeklyDash, minWeeklyWidth, minWeeklyShowMedian, minWeeklyShowLabel, minWeeklyLongLabel, minWeeklyShortLabel, currentWeeklyColor, currentWeeklyDash, currentWeeklyWidth, currentWeeklyShowMedian, currentWeeklyShowLabel, startWeekFromDay, currentWeeklyLongLabel, currentWeeklyShortLabel, minMonthlyColor, minMonthlyDash, minMonthlyWidth, minMonthlyShowMedian, minMonthlyShowLabel, minMonthlyLongLabel, minMonthlyShortLabel);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Cargocult.MultiSeriesHL MultiSeriesHL(bool min1, bool min5, bool min15, bool min30, bool min60, bool min240, bool monday, bool minDaily, bool currentDaily, bool minWeekly, bool currentWeekly, bool minMonthly, string highLongLabel, string highShortLabel, string lowLongLabel, string lowShortLabel, string medianLongLabel, string medianShortLabel, string previousLongLabel, string previousShortLabel, bool useShortLabels, bool showLabelPrices, Brush min1Color, DashStyleHelper min1Dash, int min1Width, bool min1ShowMedian, bool min1ShowLabel, string min1LongLabel, string min1ShortLabel, Brush min5Color, DashStyleHelper min5Dash, int min5Width, bool min5ShowMedian, bool min5ShowLabel, string min5LongLabel, string min5ShortLabel, Brush min15Color, DashStyleHelper min15Dash, int min15Width, bool min15ShowMedian, bool min15ShowLabel, string min15LongLabel, string min15ShortLabel, Brush min30Color, DashStyleHelper min30Dash, int min30Width, bool min30ShowMedian, bool min30ShowLabel, string min30LongLabel, string min30ShortLabel, Brush min60Color, DashStyleHelper min60Dash, int min60Width, bool min60ShowMedian, bool min60ShowLabel, string min60LongLabel, string min60ShortLabel, Brush min240Color, DashStyleHelper min240Dash, int min240Width, bool min240ShowMedian, bool min240ShowLabel, string min240LongLabel, string min240ShortLabel, Brush mondayColor, DashStyleHelper mondayDash, int mondayWidth, bool mondayShowMedian, bool mondayShowLabel, string mondayLongLabel, string mondayShortLabel, Brush minDailyColor, DashStyleHelper minDailyDash, int minDailyWidth, bool minDailyShowMedian, bool minDailyShowLabel, string minDailyLongLabel, string minDailyShortLabel, Brush currentDailyColor, DashStyleHelper currentDailyDash, int currentDailyWidth, bool currentDailyShowMedian, bool currentDailyShowLabel, string currentDailyLongLabel, string currentDailyShortLabel, Brush minWeeklyColor, DashStyleHelper minWeeklyDash, int minWeeklyWidth, bool minWeeklyShowMedian, bool minWeeklyShowLabel, string minWeeklyLongLabel, string minWeeklyShortLabel, Brush currentWeeklyColor, DashStyleHelper currentWeeklyDash, int currentWeeklyWidth, bool currentWeeklyShowMedian, bool currentWeeklyShowLabel, DayOfWeek startWeekFromDay, string currentWeeklyLongLabel, string currentWeeklyShortLabel, Brush minMonthlyColor, DashStyleHelper minMonthlyDash, int minMonthlyWidth, bool minMonthlyShowMedian, bool minMonthlyShowLabel, string minMonthlyLongLabel, string minMonthlyShortLabel)
		{
			return indicator.MultiSeriesHL(Input, min1, min5, min15, min30, min60, min240, monday, minDaily, currentDaily, minWeekly, currentWeekly, minMonthly, highLongLabel, highShortLabel, lowLongLabel, lowShortLabel, medianLongLabel, medianShortLabel, previousLongLabel, previousShortLabel, useShortLabels, showLabelPrices, min1Color, min1Dash, min1Width, min1ShowMedian, min1ShowLabel, min1LongLabel, min1ShortLabel, min5Color, min5Dash, min5Width, min5ShowMedian, min5ShowLabel, min5LongLabel, min5ShortLabel, min15Color, min15Dash, min15Width, min15ShowMedian, min15ShowLabel, min15LongLabel, min15ShortLabel, min30Color, min30Dash, min30Width, min30ShowMedian, min30ShowLabel, min30LongLabel, min30ShortLabel, min60Color, min60Dash, min60Width, min60ShowMedian, min60ShowLabel, min60LongLabel, min60ShortLabel, min240Color, min240Dash, min240Width, min240ShowMedian, min240ShowLabel, min240LongLabel, min240ShortLabel, mondayColor, mondayDash, mondayWidth, mondayShowMedian, mondayShowLabel, mondayLongLabel, mondayShortLabel, minDailyColor, minDailyDash, minDailyWidth, minDailyShowMedian, minDailyShowLabel, minDailyLongLabel, minDailyShortLabel, currentDailyColor, currentDailyDash, currentDailyWidth, currentDailyShowMedian, currentDailyShowLabel, currentDailyLongLabel, currentDailyShortLabel, minWeeklyColor, minWeeklyDash, minWeeklyWidth, minWeeklyShowMedian, minWeeklyShowLabel, minWeeklyLongLabel, minWeeklyShortLabel, currentWeeklyColor, currentWeeklyDash, currentWeeklyWidth, currentWeeklyShowMedian, currentWeeklyShowLabel, startWeekFromDay, currentWeeklyLongLabel, currentWeeklyShortLabel, minMonthlyColor, minMonthlyDash, minMonthlyWidth, minMonthlyShowMedian, minMonthlyShowLabel, minMonthlyLongLabel, minMonthlyShortLabel);
		}

		public Indicators.Cargocult.MultiSeriesHL MultiSeriesHL(ISeries<double> input , bool min1, bool min5, bool min15, bool min30, bool min60, bool min240, bool monday, bool minDaily, bool currentDaily, bool minWeekly, bool currentWeekly, bool minMonthly, string highLongLabel, string highShortLabel, string lowLongLabel, string lowShortLabel, string medianLongLabel, string medianShortLabel, string previousLongLabel, string previousShortLabel, bool useShortLabels, bool showLabelPrices, Brush min1Color, DashStyleHelper min1Dash, int min1Width, bool min1ShowMedian, bool min1ShowLabel, string min1LongLabel, string min1ShortLabel, Brush min5Color, DashStyleHelper min5Dash, int min5Width, bool min5ShowMedian, bool min5ShowLabel, string min5LongLabel, string min5ShortLabel, Brush min15Color, DashStyleHelper min15Dash, int min15Width, bool min15ShowMedian, bool min15ShowLabel, string min15LongLabel, string min15ShortLabel, Brush min30Color, DashStyleHelper min30Dash, int min30Width, bool min30ShowMedian, bool min30ShowLabel, string min30LongLabel, string min30ShortLabel, Brush min60Color, DashStyleHelper min60Dash, int min60Width, bool min60ShowMedian, bool min60ShowLabel, string min60LongLabel, string min60ShortLabel, Brush min240Color, DashStyleHelper min240Dash, int min240Width, bool min240ShowMedian, bool min240ShowLabel, string min240LongLabel, string min240ShortLabel, Brush mondayColor, DashStyleHelper mondayDash, int mondayWidth, bool mondayShowMedian, bool mondayShowLabel, string mondayLongLabel, string mondayShortLabel, Brush minDailyColor, DashStyleHelper minDailyDash, int minDailyWidth, bool minDailyShowMedian, bool minDailyShowLabel, string minDailyLongLabel, string minDailyShortLabel, Brush currentDailyColor, DashStyleHelper currentDailyDash, int currentDailyWidth, bool currentDailyShowMedian, bool currentDailyShowLabel, string currentDailyLongLabel, string currentDailyShortLabel, Brush minWeeklyColor, DashStyleHelper minWeeklyDash, int minWeeklyWidth, bool minWeeklyShowMedian, bool minWeeklyShowLabel, string minWeeklyLongLabel, string minWeeklyShortLabel, Brush currentWeeklyColor, DashStyleHelper currentWeeklyDash, int currentWeeklyWidth, bool currentWeeklyShowMedian, bool currentWeeklyShowLabel, DayOfWeek startWeekFromDay, string currentWeeklyLongLabel, string currentWeeklyShortLabel, Brush minMonthlyColor, DashStyleHelper minMonthlyDash, int minMonthlyWidth, bool minMonthlyShowMedian, bool minMonthlyShowLabel, string minMonthlyLongLabel, string minMonthlyShortLabel)
		{
			return indicator.MultiSeriesHL(input, min1, min5, min15, min30, min60, min240, monday, minDaily, currentDaily, minWeekly, currentWeekly, minMonthly, highLongLabel, highShortLabel, lowLongLabel, lowShortLabel, medianLongLabel, medianShortLabel, previousLongLabel, previousShortLabel, useShortLabels, showLabelPrices, min1Color, min1Dash, min1Width, min1ShowMedian, min1ShowLabel, min1LongLabel, min1ShortLabel, min5Color, min5Dash, min5Width, min5ShowMedian, min5ShowLabel, min5LongLabel, min5ShortLabel, min15Color, min15Dash, min15Width, min15ShowMedian, min15ShowLabel, min15LongLabel, min15ShortLabel, min30Color, min30Dash, min30Width, min30ShowMedian, min30ShowLabel, min30LongLabel, min30ShortLabel, min60Color, min60Dash, min60Width, min60ShowMedian, min60ShowLabel, min60LongLabel, min60ShortLabel, min240Color, min240Dash, min240Width, min240ShowMedian, min240ShowLabel, min240LongLabel, min240ShortLabel, mondayColor, mondayDash, mondayWidth, mondayShowMedian, mondayShowLabel, mondayLongLabel, mondayShortLabel, minDailyColor, minDailyDash, minDailyWidth, minDailyShowMedian, minDailyShowLabel, minDailyLongLabel, minDailyShortLabel, currentDailyColor, currentDailyDash, currentDailyWidth, currentDailyShowMedian, currentDailyShowLabel, currentDailyLongLabel, currentDailyShortLabel, minWeeklyColor, minWeeklyDash, minWeeklyWidth, minWeeklyShowMedian, minWeeklyShowLabel, minWeeklyLongLabel, minWeeklyShortLabel, currentWeeklyColor, currentWeeklyDash, currentWeeklyWidth, currentWeeklyShowMedian, currentWeeklyShowLabel, startWeekFromDay, currentWeeklyLongLabel, currentWeeklyShortLabel, minMonthlyColor, minMonthlyDash, minMonthlyWidth, minMonthlyShowMedian, minMonthlyShowLabel, minMonthlyLongLabel, minMonthlyShortLabel);
		}
	}
}

#endregion
