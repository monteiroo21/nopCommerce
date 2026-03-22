import http from 'k6/http';
import { check, sleep } from 'k6';
import { parseHTML } from 'k6/html';

export const options = {
    stages: [
        { duration: '30s', target: 2 },
        { duration: '1m', target: 5 },
        { duration: '1m', target: 10 },
        { duration: '1m', target: 5 },
        { duration: '30s', target: 0 },
    ],
    thresholds: {
        http_req_duration: ['p(95)<2000'],
        'checks{check:Order confirmed}': ['rate>0.9'],
    },
};

const BASE_URL = 'http://localhost:80';

const PRODUCTS = [
    { id: 5, slug: 'asus-laptop' },
    { id: 6, slug: 'samsung-premium-ultrabook' },
    { id: 7, slug: 'hp-spectre-xt-pro-ultrabook' },
];

function getCsrfToken(body) {
    if (!body) return '';
    const m = body.match(/name="__RequestVerificationToken"[^>]+value="([^"]+)"/);
    if (m) return m[1];
    const m2 = body.match(/value="([^"]+)"[^\s>]*\s+name="__RequestVerificationToken"/);
    return m2 ? m2[1] : '';
}

export default function () {
    const product = PRODUCTS[Math.floor(Math.random() * PRODUCTS.length)];

    http.get(`${BASE_URL}/`);

    let res = http.get(`${BASE_URL}/${product.slug}`);
    check(res, { 'Product page loaded': (r) => r.status === 200 });
    let csrfToken = getCsrfToken(res.body);

    res = http.post(
        `${BASE_URL}/addproducttocart/catalog/${product.id}/1/1`,
        {
            [`addtocart_${product.id}.EnteredQuantity`]: '1',
            '__RequestVerificationToken': csrfToken,
        }
    );
    check(res, { 'Added to cart': (r) => r.status === 200 });

    res = http.get(`${BASE_URL}/login/checkoutasguest?returnUrl=%2Fcheckout%2Fonepagecheckout`);
    check(res, { 'Guest checkout activated': (r) => r.status === 200 });
    csrfToken = getCsrfToken(res.body);

    res = http.get(`${BASE_URL}/onepagecheckout`);
    csrfToken = getCsrfToken(res.body);

    res = http.post(`${BASE_URL}/checkout/OpcSaveBilling`, {
        'BillingNewAddress.FirstName': 'Load',
        'BillingNewAddress.LastName': 'Tester',
        'BillingNewAddress.Email': `k6_vu${__VU}_it${__ITER}@example.com`,
        'BillingNewAddress.CountryId': '1',
        'BillingNewAddress.StateProvinceId': '0',
        'BillingNewAddress.City': 'New York',
        'BillingNewAddress.Address1': '123 Test St',
        'BillingNewAddress.ZipPostalCode': '10001',
        'BillingNewAddress.PhoneNumber': '1234567890',
        'ShipToSameAddress': 'true',
        '__RequestVerificationToken': csrfToken,
    });
    const billingOk = res.status === 200 && res.body &&
        res.json('goto_section') === 'shipping_method';
    check(res, { 'Billing saved': () => billingOk });

    const shippingHtml = (res.body && res.json('update_section.html')) || '';
    const shippingMatch = shippingHtml.match(/value="(.*?___.*?)"/);
    const shippingOption = shippingMatch
        ? shippingMatch[1]
        : 'Ground___Shipping.FixedByWeightByTotal';

    res = http.post(`${BASE_URL}/checkout/OpcSaveShippingMethod`, {
        'shippingoption': shippingOption,
        '__RequestVerificationToken': csrfToken,
    });
    check(res, { 'Shipping method saved': (r) => r.status === 200 });

    res = http.post(`${BASE_URL}/checkout/OpcSavePaymentMethod`, {
        'paymentmethod': 'Payments.CheckMoneyOrder',
        '__RequestVerificationToken': csrfToken,
    });
    check(res, { 'Payment method saved': (r) => r.status === 200 });

    res = http.post(`${BASE_URL}/checkout/OpcSavePaymentInfo`, {
        '__RequestVerificationToken': csrfToken,
    });
    check(res, { 'Payment info saved': (r) => r.status === 200 });

    res = http.post(`${BASE_URL}/checkout/OpcConfirmOrder`, {
        '__RequestVerificationToken': csrfToken,
    });
    check(res, { 'Order confirmed': (r) => r.status === 200 });

    sleep(1);
}