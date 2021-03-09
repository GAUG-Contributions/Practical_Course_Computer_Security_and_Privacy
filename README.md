# Lab Course on Computer Security and Privacy - WiSe 2020/2021

## Table Of Contents
This repository contains several projects:
- **SensorFeedback**: Main Application project. Handles sensors activation and forwards feedback messages to the associated Watch Face, depending on user feedback preferences.
- **SensorFeedbackWF**: Watch Face project. Receives feedback mesages from the main app, and generates visual, haptic and/or sonor feedback in different forms. 
- **Monitoring**: archived project, first attempt for this course. Attempted to monitor installed apps to detect sensor usage.
- **Report**: Report for this project, in LaTeX.

## Application Architecture
The basic structure of the project (Watch Face + App) is as follows:
![Diagram](appDiagram.png)