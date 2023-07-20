- [DailyPriceLevels](#dailypricelevels)
  - [LevelFileName](#levelfilename)
  - [SuperDOM](#superdom)
  - [Other config options](#other-config-options)
  - [Notes](#notes)


## DailyPriceLevels
Daily Price Levels indicator allows for the plotting of lines per day that are specified in a text file.  
The lines are loaded per day and work in historical/market replay mode as well as realtime.

To install import the DailyPriceLevels.zip file and import the script
![image](https://github.com/cargocultscience/nt-indicators/assets/28972498/e4bbcbed-1acb-417e-8196-321dfaef4aaf)

Next configure the indicator:
![image](https://github.com/cargocultscience/nt-indicators/assets/28972498/63d68f91-9315-4266-af18-84c5b3088b13)
### LevelFileName
This needs to be a text file that can be anywhere accessable by your windows machine.  
The format of the file is 
```
date | levels
yyyy-mm-dd | p1, p2, p3, p4
yyyy-mm-dd | p1, p2, p3, p5, p6, p7
```
* first line (header) of the file will always be ignored
* you may have as many dates as you wish in this file
* you may have as many levels as you wish in this file

For example if we create a file "es_example_levels.csv"
```
date | levels
2023-07-19 | 4580, 4600.50, 4610.25
```
You should see the following:

![image](https://github.com/cargocultscience/nt-indicators/assets/28972498/7f486dc4-05cb-44c8-a12c-59931c6ce8b2)

### SuperDOM
This indicator will also work in the SuperDOM!  Simply right click on the SuperDom and select "Indicators..."

![image](https://github.com/cargocultscience/nt-indicators/assets/28972498/27ce1814-6ef0-4bcc-87bd-a3f876c90d8e)

Configure the indicator in the same way as for your chart.  Once configured the indicator will be displayed in this fashion:

![image](https://github.com/cargocultscience/nt-indicators/assets/28972498/012694cc-e200-4aac-a0f8-ab8c7de5d569)

### Other config options
* Level Scale Factor - this was something i experimented with.  The idea was to plot SPY levels on an ES chart using a scale factor.  I have since abandoned this appraoch and favor connecting to ThinkOrSwim for equity data and plotting the values directly in another pane or separate chart.

### Notes
* You can add to the text file without reloading the indicator.  This works provided you do not exceed 25 levels.   In the event that you exceed 25 levels you will need to reload the indicator.  The easiest way to do this I have found is hitting 'F5'
* If you experience problems check the NT8 log tab.  Here is an example of what you will see if the level file does not exist ![image](https://github.com/cargocultscience/nt-indicators/assets/28972498/148143ed-ac9c-4d33-9c4a-6bd792cc00ce)







