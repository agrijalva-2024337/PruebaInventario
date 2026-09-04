namespace SLCDM.Application.Common.Interfaces;

public interface IDeviceTokenHashService
{
    string GenerateRawToken();

    string Hash(string rawToken);
}
