using System;
using System.Configuration;
using RabbitMQ.Client;
using Xunit;

namespace Core.Tests
{
	public class RequiresRabbitFactAttribute : FactAttribute
	{
		public RequiresRabbitFactAttribute()
		{
			if (IsRabbitAvailable.Value == false)
			{
				Skip = "RabbitMQ is not available";
			}
		}

		private static readonly Lazy<bool> IsRabbitAvailable;

		static RequiresRabbitFactAttribute()
		{
			IsRabbitAvailable = new Lazy<bool>(() =>
			{
				var factory = new ConnectionFactory
				{
					HostName = ConfigurationManager.AppSettings["RabbitHost"],
					RequestedConnectionTimeout = 1000
				};

				try
				{
					using (var connection = factory.CreateConnection())
					{
						return connection.IsOpen;
					}
				}
				catch (Exception)
				{
					return false;
				}
			});
		}
	}
}
