﻿using System.Globalization;
using System.Runtime.CompilerServices;
using Newtonsoft.Json.Linq;
using TomorrowIoApi.Exceptions;
using TomorrowIoApi.Models.Timelines;
using WeatherApi;

[assembly: InternalsVisibleToAttribute("TomorrowIoApi.UnitTests")]
namespace TomorrowIoApi;

public class TomorrowIo : AWeatherApi
{
	protected override string BaseUrl => "https://api.tomorrow.io/v4";
	protected override string TokenName => "apikey";

	private static readonly string[] DefaultFields = {
		"temperature",
		"cloudCover",
		"humidity",
		"precipitationIntensity",
		"windSpeed",
		"windDirection"
	};
	
	public TomorrowIo(string token) : base(token) {}

	// For tests only
	internal TomorrowIo(string token, HttpClient httpClient) : base(token, httpClient) {}

	public async Task<List<TimelineModel>> GetTimelinesAsync(
		(double lat, double lon) location,
		string[] timesteps,
		string[]? fields = null,
		string units = "metric" 
	)
	{
		string lat = location.lat.ToString(CultureInfo.InvariantCulture);
		string lon = location.lon.ToString(CultureInfo.InvariantCulture);
		
		fields ??= DefaultFields;
		
		var options = new Dictionary<string, string>
		{
			["location"] = $"{lat},{lon}",
			["fields"] = string.Join(",", fields),
			["timesteps"] = string.Join(",", timesteps),
			["units"] = units
		};

		JToken timelinesJson = (await CallMethodAsync("timelines", options))["data"]!["timelines"]!;
		List<TimelineModel>? timelines = timelinesJson.ToObject<List<TimelineModel>>();
		return timelines ?? new List<TimelineModel>();
	}

	public override Task<Weather> GetCurrentAsync((double lat, double lon) location) =>
		GetCurrentAsync(location);
	
	public async Task<Weather> GetCurrentAsync(
		(double lat, double lon) location,
		string[]? fields = null,
		string units = "metric"
	)
	{
		IntervalModel? interval = (await GetTimelinesAsync(
			location,
			new []{ "current" },
			fields,
			units
		))[0].Intervals?[0];
		
		return interval?.ToWeather() ?? new();
	}
	
	protected override void HandleErrors(JObject responseJson)
	{
		if (responseJson["code"] is not null)
			throw new ApiException(
				(int)responseJson["code"]!,
				(string)responseJson["type"]!,
				(string)responseJson["message"]!
			);
	}
}