import { test, expect } from '@playwright/test';

test('has title and login card renders correctly', async ({ page }) => {
  await page.goto('/');

  await expect(page).toHaveTitle(/Pisicile Sălbatice/);

  await expect(page.locator('h1').filter({ hasText: 'Pisicile Sălbatice' })).toBeVisible();
  await expect(page.locator('h3').filter({ hasText: 'Autentificare' })).toBeVisible();
});

test('structural validation detects empty fields immediately', async ({ page }) => {
  await page.goto('/');

  await page.locator('button.btn-main').filter({ hasText: 'Intră în cont' }).click();

  await expect(page.locator('small.val-error').filter({ hasText: 'Numele de utilizator este necesar' })).toBeVisible();
  await expect(page.locator('small.val-error').filter({ hasText: 'Parola' })).toBeVisible();
});

test('end-to-end registration flow bypasses duplicate username conflict', async ({ page }) => {
  await page.goto('/');

  await page.locator('.toggle-link').click();
  await expect(page.locator('h3').filter({ hasText: 'Creează Cont' })).toBeVisible();

  const randomId = Date.now() + Math.floor(Math.random() * 1000);
  await page.locator('input[name="username"]').fill(`PlaywrightUser${randomId}`);
  await page.locator('input[name="firstName"]').fill('John');
  await page.locator('input[name="lastName"]').fill('Doe');
  await page.locator('input[name="email"]').fill(`doe${randomId}@example.com`);
  await page.locator('input[name="dateOfBirth"]').fill('1990-01-01');
  await page.locator('input[name="password"]').fill('secretpw123');

  page.on('dialog', dialog => dialog.accept());

  await page.locator('button.btn-main').filter({ hasText: 'Înregistrare' }).click();

  await expect(page.locator('h3').filter({ hasText: 'Autentificare' })).toBeVisible();
  
  await page.locator('input[name="username"]').fill(`PlaywrightUser${randomId}`);
  await page.locator('input[name="password"]').fill('secretpw123');
  await page.locator('button.btn-main').filter({ hasText: 'Intră în cont' }).click();

  await expect(page.locator('h1').filter({ hasText: `Salut, PlaywrightUser${randomId}!` })).toBeVisible();
  await expect(page.locator('.logout-btn')).toBeVisible();
});
