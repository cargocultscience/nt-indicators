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

#endregion

//This namespace holds MarketAnalyzerColumns in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public class CargocultVWAPx : MarketAnalyzerColumn
	{
		private NinjaTrader.NinjaScript.Indicators.CargocultVWAPx indicator;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Market Analyzer Column For VWAP";
				Name										= "VWAP";
				Calculate									= Calculate.OnBarClose;
				DataType   									= typeof(double);
        		IsEditable 									= false;
				FormatDecimals 								= 2;
			}
			else if (State == State.DataLoaded)
			{
				indicator		= CargocultVWAPx();
				
			}
		}

		public enum ValueTypeEnum
		{
			Value,
			TicksFromValue,
		}

		
		protected override void OnMarketData(MarketDataEventArgs marketDataUpdate)
		{
			if (marketDataUpdate.IsReset)
				CurrentValue = double.NaN;
			
			if (marketDataUpdate.MarketDataType == MarketDataType.Last)
			{
			  	switch(ValueType) 
			  	{				
					case ValueTypeEnum.Value:
						CurrentValue = indicator.PlotVWAP[0]; break;
					case ValueTypeEnum.TicksFromValue:
						CurrentValue = Math.Round((marketDataUpdate.Price - indicator.PlotVWAP[0]) / Instrument.MasterInstrument.TickSize); break;
				}
			}
		}

		
		#region Properties
		[NinjaScriptProperty]
		[Display(Name="Display Value", Description="Name of the csv file containing the price levels", Order=2, GroupName="Level Parameters")]
		public ValueTypeEnum ValueType
		{ get; set; }
		
		#endregion
	}
}
