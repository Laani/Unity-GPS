using System; 
using UnityEngine;
using System.Collections; 
using System.Collections.Generic;
using MiniJSON;


public class NetworkHelper:MonoBehaviour
{
	public int numStrokes = 0;
	public int numPoints = 0;

	public static class Haversine {
		/* Calculate the distance between 2 points in 3D.  This is a little sketchy because the distance between 2 points in 3D
		 * is illdefined if you aren't going to stay at the same altitude as you travel between the two points.  The
		 * curvature of the earth starts to mess with this */
		public static double calculate(double lat1, double lng1, double alt1,double lat2, double lng2,double alt2) {
			var R = 6372800; // In meters
			var dLat = toRadians(lat2 - lat1);
			var dLon = toRadians(lng2 - lng1);
			lat1 = toRadians(lat1);
			lat2 = toRadians(lat2);
			
			var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) + Math.Sin(dLon / 2) * Math.Sin(dLon / 2) * Math.Cos(lat1) * Math.Cos(lat2);
			var c = 2 * Math.Atan2(Math.Sqrt(a),Math.Sqrt(1-a));
			double distance = R * c;

			double height = alt1 - alt2;
			distance = Math.Pow (distance,2) + Math.Pow (height,2);
			return Math.Sqrt (distance);


		}
		
		public static double toRadians(double angle) {
			return Math.PI * angle / 180.0;
		}
	}

	private class Point{
		long timestamp;
		double lat;
		double lng;
		double alt;

		public Point(long timestamp,double lat, double lng,double alt){
			this.timestamp = timestamp;
			this.lat = lat;
			this.lng = lng;
			this.alt = alt;
		}

		public double distance(Point other){
			return Haversine.calculate (this.lat, this.lng, this.alt, other.lat, other.lng, other.alt);
		}

		public Dictionary<string,string> toDictionary(){
			Dictionary<string,string> ret = new Dictionary<string,string> ();
			ret.Add ("time", this.timestamp + "");
			ret.Add ("lat", this.lat + "");
			ret.Add ("lng", this.lng + "");
			ret.Add ("alt", this.alt + "");
			return ret;
		}
	}
	

	Dictionary<string,List<Point>> strokePoints = new Dictionary<string,List<Point>>();
	Dictionary<string,Color> strokeColors = new Dictionary<string,Color>();

	public void addPoint(string stroke,long timestamp,double lat, double lng, double alt){
		Debug.Log ("addPoint called");
		Point newPoint = new Point (timestamp, lat, lng, alt);

		/* Get the existing stroke or start a new one */
		List<Point> points;
		if (!strokePoints.TryGetValue (stroke, out points)) {
			points = new List<Point> ();
			numStrokes++;
		}
		numPoints -= points.Count;

		/* Check if the new point is sufficiently far away */
		int farEnough = 10; //Meters
		if (points.Count == 0) {
			points.Add (newPoint);
		} else {
			Point lastPoint = points.FindLast (delegate(Point obj) {
				return true;
			});
			if (newPoint.distance (lastPoint) > farEnough) {
				points.Add (newPoint);
			}
			else{
				Debug.Log("Point rejected for being too close");
			}
		}
		strokePoints[stroke] = points;
		numPoints += points.Count;
	}

	public void addStrokeColor(string stroke, Color color){
		strokeColors[stroke]= color;
	}


	string ADD_STROKE_URL = "http://djp3-pc2.ics.uci.edu:9020/add_stroke";

	public void uploadPoints(string group_name,string drawing_name){
		WWW www;
		List<Dictionary<string,string>> jsonOutbound;
		List<string> deleteUs = new List<string> ();

		foreach(KeyValuePair<string, List<Point>> entry in strokePoints)
		{
			List<Point> record = entry.Value;
	
			while(record.Count > 0){

				string u = ADD_STROKE_URL;
				u += "?group_name=" + WWW.EscapeURL (group_name);
				u += "&drawing_name=" + WWW.EscapeURL (drawing_name);
				u += "&stroke_name=" + WWW.EscapeURL (entry.Key);
				Color strokeColor;
				if(!strokeColors.TryGetValue(entry.Key,out strokeColor)){
					strokeColor = new Color(0,0,0);
				}
				u += "&red=" + WWW.EscapeURL (((int)(strokeColor[0]*255.0))+"");
				u += "&green=" + WWW.EscapeURL (((int)(strokeColor[1]*255.0))+"");
				u += "&blue=" + WWW.EscapeURL (((int)(strokeColor[2]*255.0))+"");

				int chunk = 0;
				jsonOutbound = new List<Dictionary<string,string>>();
				List<Point> deletePoints = new List<Point>();
				foreach(Point p in record){
					if(chunk < 250){
						jsonOutbound.Add(p.toDictionary());
						chunk++;
						deletePoints.Add (p);
					}
				}
 				u += "&stroke=" + WWW.EscapeURL(Json.Serialize(jsonOutbound));
				www = new WWW (u);
				StartCoroutine (WaitForRequest (www));
				if(www.error == null){
					//Everything went ok
					foreach (Point p in deletePoints){
						record.Remove (p);
					}
				}
				else{
					//network error
					updateCounts ();
					return;
				}
			}
			deleteUs.Add (entry.Key);


		}

		foreach (string x in deleteUs) {
			strokePoints.Remove (x);
		}

		updateCounts ();

	}

	private void updateCounts(){
		//Update counts
		numStrokes = strokePoints.Keys.Count;
		numPoints = 0;
		foreach (KeyValuePair<string, List<Point>> x in strokePoints) {
			numPoints += x.Value.Count;
		}
	}

		
	private IEnumerator WaitForRequest (WWW www)
	{
		yield return www;
			
		// check for errors
		if (www.error == null) {
			Debug.Log ("WWW Ok!: " + www.text);
		} else {
			Debug.Log ("WWW Error: " + www.error);
		}    
	}
}
