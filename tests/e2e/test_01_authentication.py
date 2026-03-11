"""Authentication tests for XAF Blazor Server."""
import pytest
from playwright.sync_api import Page, expect
import sys
sys.path.insert(0, "..")
from conftest import xaf_login, xaf_logout, BASE_URL, log


@pytest.mark.auth
class TestAuthentication:
    def test_admin_login(self, page: Page):
        """Admin can log in with empty password."""
        xaf_login(page, "Admin")
        expect(page.locator(".xaf-sidebar")).to_be_visible()

    def test_user_login(self, page: Page):
        """User can log in with empty password."""
        xaf_login(page, "User")
        expect(page.locator(".xaf-sidebar")).to_be_visible()

    def test_manager_login(self, page: Page):
        """Manager can log in with empty password."""
        xaf_login(page, "Manager")
        expect(page.locator(".xaf-sidebar")).to_be_visible()

    def test_invalid_password(self, page: Page):
        """Login with wrong password shows error / stays on login page."""
        log.info("Testing invalid password")
        page.goto(BASE_URL)
        page.wait_for_timeout(3000)
        page.locator('input.dxbl-text-edit-input[type="text"]').first.fill("Admin")
        page.locator('input.dxbl-text-edit-input[type="password"]').first.fill("wrongpassword")
        page.click('button[data-action-name="Log In"]')
        page.wait_for_timeout(3000)
        # Should still be on login page
        expect(page.locator('input.dxbl-text-edit-input[type="text"]').first).to_be_visible()

    def test_logout(self, page: Page):
        """User can log out and is redirected to login page."""
        xaf_login(page, "Admin")
        xaf_logout(page)
        expect(page.locator('input.dxbl-text-edit-input[type="text"]').first).to_be_visible()
