"""Dashboard creation tests — Admin creates dashboard with Custom SQL."""
import pytest
from playwright.sync_api import Page, expect
import sys
sys.path.insert(0, "..")
from conftest import (
    xaf_login,
    xaf_logout,
    navigate_to,
    get_dashboard_titles,
    BASE_URL,
)


def navigate_to_dashboards(page: Page):
    """Navigate to the SecureDashboardData list view."""
    navigate_to(page, "Reports")
    page.click('.xaf-navigation >> text="Secure Dashboard Data"')
    page.wait_for_selector(".xaf-listview", timeout=15_000)


@pytest.mark.creation
class TestDashboardCreation:
    def test_admin_creates_dashboard(self, page: Page):
        """Admin creates a new dashboard entry."""
        xaf_login(page, "Admin")
        navigate_to_dashboards(page)

        # Click New
        page.click('[data-action-id="New"]')
        page.wait_for_selector(".xaf-detailview", timeout=15_000)

        # Fill in Title field
        title_input = page.locator(
            '.xaf-detailview [data-field-name="Title"] input, '
            '.xaf-detailview [data-field-name="Title"] .dxbl-edit input'
        ).first
        title_input.fill("Test SQL Dashboard")

        # Save
        page.click('[data-action-id="Save"]')
        page.wait_for_timeout(2000)

        # Go back to list and verify it appears
        navigate_to_dashboards(page)
        titles = get_dashboard_titles(page)
        assert "Test SQL Dashboard" in titles

    def test_new_unrestricted_dashboard_visible_to_all(self, page: Page):
        """A new dashboard with no role assignment should be visible to all users."""
        # User should see it
        xaf_login(page, "User")
        titles = get_dashboard_titles(page)
        assert "Test SQL Dashboard" in titles, (
            f"User should see unrestricted dashboard. Visible: {titles}"
        )
        xaf_logout(page)

        # Manager should see it too
        xaf_login(page, "Manager")
        titles = get_dashboard_titles(page)
        assert "Test SQL Dashboard" in titles, (
            f"Manager should see unrestricted dashboard. Visible: {titles}"
        )

    def test_restrict_new_dashboard_to_role(self, page: Page):
        """After assigning a role, only that role sees the new dashboard."""
        xaf_login(page, "Admin")

        # Create assignment: Test SQL Dashboard -> Default role only
        navigate_to(page, "Administration")
        page.click('.xaf-navigation >> text="Dashboard Role Assignment"')
        page.wait_for_selector(".xaf-listview", timeout=15_000)
        page.click('[data-action-id="New"]')
        page.wait_for_selector(".xaf-detailview", timeout=15_000)

        # Set Dashboard
        dashboard_lookup = page.locator(
            '.xaf-detailview [data-field-name="Dashboard"] .dxbl-edit'
        ).first
        dashboard_lookup.click()
        page.wait_for_timeout(500)
        page.locator('.dxbl-listbox >> text="Test SQL Dashboard"').click()
        page.wait_for_timeout(500)

        # Set Role = Default
        role_lookup = page.locator(
            '.xaf-detailview [data-field-name="Role"] .dxbl-edit'
        ).first
        role_lookup.click()
        page.wait_for_timeout(500)
        page.locator('.dxbl-listbox >> text="Default"').click()
        page.wait_for_timeout(500)

        page.click('[data-action-id="Save"]')
        page.wait_for_timeout(2000)

        # User (Default) should still see it
        xaf_logout(page)
        xaf_login(page, "User")
        titles = get_dashboard_titles(page)
        assert "Test SQL Dashboard" in titles

        # Manager should NOT see it anymore (restricted to Default only)
        xaf_logout(page)
        xaf_login(page, "Manager")
        titles = get_dashboard_titles(page)
        assert "Test SQL Dashboard" not in titles, (
            f"Manager should not see dashboard restricted to Default. Visible: {titles}"
        )

    def test_cleanup_delete_test_dashboard(self, page: Page):
        """Admin deletes the test dashboard — cleanup for other test suites."""
        xaf_login(page, "Admin")

        # First delete the role assignment
        navigate_to(page, "Administration")
        page.click('.xaf-navigation >> text="Dashboard Role Assignment"')
        page.wait_for_selector(".xaf-listview", timeout=15_000)
        test_row = page.locator(
            '.xaf-listview [class*="GridRow"]:has-text("Test SQL Dashboard")'
        ).first
        if test_row.is_visible():
            test_row.click()
            page.wait_for_selector(".xaf-detailview", timeout=15_000)
            page.click('[data-action-id="Delete"]')
            page.wait_for_timeout(500)
            confirm = page.locator('button:has-text("Yes"), button:has-text("OK")')
            if confirm.is_visible():
                confirm.click()
            page.wait_for_timeout(1000)

        # Now delete the dashboard itself
        navigate_to_dashboards(page)
        test_dash_row = page.locator(
            '.xaf-listview [class*="GridRow"]:has-text("Test SQL Dashboard")'
        ).first
        if test_dash_row.is_visible():
            test_dash_row.click()
            page.wait_for_selector(".xaf-detailview", timeout=15_000)
            page.click('[data-action-id="Delete"]')
            page.wait_for_timeout(500)
            confirm = page.locator('button:has-text("Yes"), button:has-text("OK")')
            if confirm.is_visible():
                confirm.click()
            page.wait_for_timeout(1000)

        # Verify it's gone
        navigate_to_dashboards(page)
        titles = get_dashboard_titles(page)
        assert "Test SQL Dashboard" not in titles
