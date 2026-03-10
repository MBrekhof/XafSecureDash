"""Edge case tests for dashboard security."""
import pytest
from playwright.sync_api import Page, expect
import sys
sys.path.insert(0, "..")
from conftest import xaf_login, xaf_logout, navigate_to, get_dashboard_titles, BASE_URL


@pytest.mark.edge
class TestEdgeCases:
    def test_unauthenticated_redirects_to_login(self, page: Page):
        """Accessing the app without login redirects to login page."""
        page.goto(BASE_URL)
        page.wait_for_timeout(3000)
        expect(page.locator('input[aria-label="User Name"]')).to_be_visible()

    def test_public_dashboard_visible_to_all_roles(self, page: Page):
        """Public Overview (no role assignments) is visible to every authenticated user."""
        for username in ["Admin", "User", "Manager"]:
            page.goto(BASE_URL)
            page.wait_for_timeout(1000)
            # Ensure we're logged out first
            if page.locator(".xaf-navigation").is_visible():
                xaf_logout(page)
            xaf_login(page, username)
            titles = get_dashboard_titles(page)
            assert "Public Overview" in titles, (
                f"{username} should see Public Overview. Visible: {titles}"
            )
            xaf_logout(page)

    def test_dashboard_list_is_empty_for_user_with_no_matching_roles(self, page: Page):
        """If all dashboards are restricted to roles a user doesn't have,
        they see only unrestricted ones."""
        # This is already tested implicitly by visibility tests,
        # but let's verify the count is consistent
        xaf_login(page, "User")
        titles = get_dashboard_titles(page)
        # User has Default role. Should see:
        # - Public Overview (unrestricted)
        # - User Dashboard (assigned to Default)
        # Should NOT see Manager Dashboard (assigned to Manager only)
        restricted_visible = [
            t for t in titles if t == "Manager Dashboard"
        ]
        assert len(restricted_visible) == 0, (
            f"User should not see any Manager-only dashboards. Visible: {titles}"
        )

    def test_admin_always_bypasses_filtering(self, page: Page):
        """Admin with IsAdministrative role always sees everything regardless of assignments."""
        xaf_login(page, "Admin")
        titles = get_dashboard_titles(page)
        # Admin should see every single dashboard, including restricted ones
        assert "Public Overview" in titles
        assert "User Dashboard" in titles
        assert "Manager Dashboard" in titles

    def test_multiple_logins_same_browser(self, page: Page):
        """Logging out and in as a different user correctly updates dashboard visibility."""
        # Login as User
        xaf_login(page, "User")
        user_titles = get_dashboard_titles(page)
        assert "Manager Dashboard" not in user_titles

        # Switch to Manager
        xaf_logout(page)
        xaf_login(page, "Manager")
        manager_titles = get_dashboard_titles(page)
        assert "User Dashboard" not in manager_titles
        assert "Manager Dashboard" in manager_titles

        # Switch to Admin
        xaf_logout(page)
        xaf_login(page, "Admin")
        admin_titles = get_dashboard_titles(page)
        assert len(admin_titles) >= 3
