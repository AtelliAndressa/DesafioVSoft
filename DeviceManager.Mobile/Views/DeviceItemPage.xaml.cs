using DeviceManager.Mobile.Interfaces;
using DeviceManager.Mobile.Models;
using System.Diagnostics;

namespace DeviceManager.Mobile.Views;

[QueryProperty(nameof(Dispositivo), "Dispositivo")]
public partial class DeviceItemPage : ContentPage
{
	private readonly IDeviceRepository _deviceRepository;
	private Dispositivo _dispositivo;
	private bool _isNewItem;

	public Dispositivo Dispositivo
	{
		get => _dispositivo;
		set
		{
			_isNewItem = IsNewItem(value);
			_dispositivo = value;
			OnPropertyChanged();
		}
	}

	public DeviceItemPage(IDeviceRepository deviceRepository)
	{
		InitializeComponent();
		_deviceRepository = deviceRepository;
		BindingContext = this;
	}

	bool IsNewItem(Dispositivo dispositivo)
	{
		if (string.IsNullOrWhiteSpace(dispositivo.Descricao) && string.IsNullOrWhiteSpace(dispositivo.CodigoReferencia))
			return true;
		return false;
	}

	async void OnSaveButtonClicked(object sender, EventArgs e)
	{
		try
		{
			if (string.IsNullOrWhiteSpace(Dispositivo.CodigoReferencia))
			{
				await DisplayAlert("Erro", "O código de referência é obrigatório", "OK");
				return;
			}

			if (string.IsNullOrWhiteSpace(Dispositivo.Descricao))
			{
				await DisplayAlert("Erro", "A descrição é obrigatória", "OK");
				return;
			}

			if (_isNewItem)
			{
				Debug.WriteLine($"Adicionando novo dispositivo: {Dispositivo.CodigoReferencia}");
				await _deviceRepository.AddAsync(Dispositivo);
			}
			else
			{
				Debug.WriteLine($"Atualizando dispositivo: {Dispositivo.CodigoReferencia}");
				await _deviceRepository.UpdateAsync(Dispositivo);
			}

			await Shell.Current.GoToAsync("..");
		}
		catch (Exception ex)
		{
			Debug.WriteLine($"Erro ao salvar dispositivo: {ex.Message}");
			await DisplayAlert("Erro", ex.Message, "OK");
		}
	}

	async void OnDeleteButtonClicked(object sender, EventArgs e)
	{
		try
		{
			var confirm = await DisplayAlert("Confirmar", "Deseja realmente excluir este dispositivo?", "Sim", "Não");
			if (!confirm) return;

			Debug.WriteLine($"Excluindo dispositivo: {Dispositivo.CodigoReferencia}");
			await _deviceRepository.DeleteAsync(Dispositivo);
			await Shell.Current.GoToAsync("..");
		}
		catch (Exception ex)
		{
			Debug.WriteLine($"Erro ao excluir dispositivo: {ex.Message}");
			await DisplayAlert("Erro", ex.Message, "OK");
		}
	}

	async void OnCancelButtonClicked(object sender, EventArgs e)
	{
		await Shell.Current.GoToAsync("..");
	}
}