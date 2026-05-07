// <copyright file="PaymentAPIProxy.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using BookingBoardGames.Data;
using BookingBoardGames.Data.Interfaces;

public class PaymentAPIProxy : IPaymentRepository
{
    private readonly HttpClient httpClient;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public PaymentAPIProxy(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task<IReadOnlyList<Payment>> GetAllPaymentsAsync()
    {
        return await this.httpClient.GetFromJsonAsync<List<Payment>>("api/payments", JsonOptions)
               ?? new List<Payment>();
    }

    public async Task<Payment?> GetPaymentByIdentifierAsync(int paymentId)
    {
        var response = await this.httpClient.GetAsync($"api/payments/{paymentId}");
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<Payment>(JsonOptions);
    }

    public async Task<int> AddPaymentAsync(Payment payment)
    {
        if (payment.DateOfTransaction == default)
        {
            payment.DateOfTransaction = DateTime.Now;
        }

        var response = await this.httpClient.PostAsJsonAsync("api/payments", payment, JsonOptions);
        response.EnsureSuccessStatusCode();

        var raw = await response.Content.ReadAsStringAsync();
        return int.Parse(raw, System.Globalization.CultureInfo.InvariantCulture);
    }

    public Task<bool> DeletePaymentAsync(Payment payment)
    {
        throw new NotSupportedException("Delete is not supported by the payments API.");
    }

    public Task<Payment?> UpdatePaymentAsync(Payment payment)
    {
        throw new NotSupportedException("Update is not supported by the payments API.");
    }
}
