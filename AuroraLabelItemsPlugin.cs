﻿using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using vatsys;
using vatsys.Plugin;
using System.Collections.Concurrent;
using System.ComponentModel.Composition; //<--Need to add a reference to System.ComponentModel.Composition

//Note the reference to vatsys (set Copy Local to false) ----->

namespace AuroraLabelItemsPlugin
{
    [Export(typeof(IPlugin))]
    public class AuroraLabelItemsPlugin : ILabelPlugin, IStripPlugin
    {
        /// The name of the custom label item we've added to Labels.xml in the Profile
        const string LABEL_ITEM_LEVEL = "AURORA_LEVEL";
        const string LABEL_ITEM_3DIGIT_GROUNDSPEED = "AURORA_GROUNDSPEED";
        const string STRIP_ITEM_CALLSIGN = "AURORA_CALLSIGN";
        readonly static CustomColour EastboundColour = new CustomColour(255, 125, 0);
        readonly static CustomColour WestboundColour = new CustomColour(0, 125, 255);
        readonly ConcurrentDictionary<string, bool> eastboundCallsigns = new ConcurrentDictionary<string, bool>();

        /// Plugin Name
        public string Name { get => "Aurora Label Items"; }

        /// This is called each time a flight data record is updated
        /// Here we are updating the eastbound callsigns dictionary with each flight data record
        public void OnFDRUpdate(FDP2.FDR updated)
        {
            if (FDP2.GetFDRIndex(updated.Callsign) == -1) //FDR was removed (that's what triggered the update)
                eastboundCallsigns.TryRemove(updated.Callsign, out _);
            else
            {
                if(updated.ParsedRoute.Count > 1)
                {
                    //calculate track from first route point to last (Departure point to destination point)
                    var rte = updated.ParsedRoute;
                    double trk = Conversions.CalculateTrack(rte.First().Intersection.LatLong, rte.Last().Intersection.LatLong);
                    bool east = trk >= 0 && trk < 180;
                    eastboundCallsigns.AddOrUpdate(updated.Callsign, east, (c,e) => east);
                }
            }
        }

        /// This is called each time a radar track is updated
        public void OnRadarTrackUpdate(RDP.RadarTrack updated)
        {

        }

        /// vatSys calls this function when it encounters a custom label item (defined in Labels.xml) during the label rendering.
        /// itemType is the value of the Type attribute in Labels.xml
        /// If it's not our item being called (another plugins, for example), return null.
        /// As a general rule, don't do processing in here as you'll slow down the ASD refresh. In the case of parsing a level to a string though, that's fine.
        public CustomLabelItem GetCustomLabelItem(string itemType, Track track, FDP2.FDR flightDataRecord, RDP.RadarTrack radarTrack)
        {
            if (flightDataRecord == null)
                return null;

            switch (itemType)
            {
                case LABEL_ITEM_LEVEL:
                    int level = radarTrack == null ? flightDataRecord.PRL / 100 : radarTrack.CorrectedAltitude / 100;
                    string sLevel = level.ToString("D3");
                    if (level > RDP.TRANSITION_ALTITUDE)//then flight level
                        sLevel = "F" + sLevel;
                    else
                        sLevel = "A" + sLevel;

                    return new CustomLabelItem()
                    {
                        Text = sLevel
                    };
                case LABEL_ITEM_3DIGIT_GROUNDSPEED:
                    //get groundspeed value from either FDR or radarTrack if coupled
                    var gs = radarTrack == null ? flightDataRecord.PredictedPosition.Groundspeed : radarTrack.GroundSpeed;
                    return new CustomLabelItem()
                    {
                        Text = gs.ToString("000")//format as 3 digits (with leading zeros)
                    };
                default:
                    return null;
            }
        }

        public CustomColour SelectASDTrackColour(Track track)
        {
            //only apply East/West colour to jurisdiction state
            if (track.State != MMI.HMIStates.Jurisdiction)
                return null;

            var fdr = track.GetFDR();
            //if track doesn't have an FDR coupled do nothing
            if (fdr == null)
                return null;

            //read our dictionary of stored bools (true means is easterly) and return the correct colour
            return GetDirectionColour(fdr.Callsign);
        }

        public CustomColour SelectGroundTrackColour(Track track)
        {
            return null;
        }

        private CustomColour GetDirectionColour(string callsign)
        {
            if (eastboundCallsigns.TryGetValue(callsign, out bool east))
            {
                if (east)
                    return EastboundColour;
                else
                    return WestboundColour;
            }

            return null;
        }

        public CustomStripItem GetCustomStripItem(string itemType, Track track, FDP2.FDR flightDataRecord, RDP.RadarTrack radarTrack)
        {
            if (flightDataRecord is null)
                return null;

            switch(itemType)
            {
                case STRIP_ITEM_CALLSIGN:
                    var callsignColour = GetDirectionColour(flightDataRecord.Callsign);
                    return new CustomStripItem()
                    {
                        BackColourIdentity = Colours.Identities.StripText,
                        //only apply East/West colour to jurisdiction state
                        ForeColourIdentity = track.State != MMI.HMIStates.Jurisdiction ? Colours.Identities.StripBackground : Colours.Identities.Custom,
                        CustomForeColour = callsignColour,
                        Text = flightDataRecord.Callsign
                    };
                default:
                    return null;
            }
        }
    }
}
