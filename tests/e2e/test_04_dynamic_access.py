"""Dynamic access change tests — admin modifies assignments, users see updated lists."""
import pytest
from playwright.sync_api import Page, expect
import sys
sys.path.insert(0, "..")
from conftest import xaf_login, xaf_logout, navigate_to, get_dashboard_titles, BASE_URL


def open_role_assignment_list(page: Page):
    """Navigate to DashboardRoleAssignment list as admin."""
    navigate_to(page, "Administration")
    page.click('.xaf-navigation >> text="Dashboard Role Assignment"')
    page.wait_for_selector(".xaf-listview", timeout=15_000)


def create_role_assignment(page: Page, dashboard_title: str, role_name: str):
    """Create a new DashboardRoleAssignment via the XAF UI."""
    page.click('[data-action-id="New"]')
    page.wait_for_selector(".xaf-detailview", timeout=15_000)

    # Set Dashboard lookup
    dashboard_lookup = page.locator(
        '.xaf-detailview [data-field-name="Dashboard"] .dxbl-edit'
    ).first
    dashboard_lookup.click()
    page.wait_for_timeout(500)
    page.locator(f'.dxbl-listbox >> text="{dashboard_title}"').click()
    page.wait_for_timeout(500)

    # Set Role lookup
    role_lookup = page.locator(
        '.xaf-detailview [data-field-name="Role"] .dxbl-edit'
    ).first
    role_lookup.click()
    page.wait_for_timeout(500)
    page.locator(f'.dxbl-listbox >> text="{role_name}"').click()
    page.wait_for_timeout(500)

    # Save
    page.click('[data-action-id="Save"]')
    page.wait_for_timeout(2000)


def delete_last_role_assignment(page: Page):
    """Delete the last DashboardRoleAssignment in the list."""
    rows = page.locator('.xaf-listview [class*="GridRow"]')
    last_row = rows.last
    last_row.click()
    page.wait_for_selector(".xaf-detailview", timeout=15_000)
    page.click('[data-action-id="Delete"]')
    # Confirm deletion dialog
    page.wait_for_timeout(500)
    confirm_btn = page.locator('button:has-text("Yes"), button:has-text("OK")')
    if confirm_btn.is_visible():
        confirm_btn.click()
    page.wait_for_timeout(1000)


@pytest.mark.dynamic
class TestDynamicAccessChanges:
    def test_grant_manager_access_to_user_dashboard(self, page: Page):
        """Admin grants Manager role access to User Dashboard.
        Manager should then see 3 dashboards."""
        xaf_login(page, "Admin")
        open_role_assignment_list(page)
        create_role_assignment(page, "User Dashboard", "Manager")

        # Now log out and log in as Manager
        xaf_logout(page)
        xaf_login(page, "Manager")
        titles = get_dashboard_titles(page)
        assert "User Dashboard" in titles, f"Manager should now see User Dashboard. Visible: {titles}"

        # Cleanup: log back as Admin and delete the assignment we just created
        xaf_logout(page)
        xaf_login(page, "Admin")
        open_role_assignment_list(page)
        delete_last_role_assignment(page)

    def test_revoke_user_access_to_user_dashboard(self, page: Page):
        """Admin removes Default role from User Dashboard.
        User should then NOT see User Dashboard (only Public Overview)."""
        # First verify User currently sees User Dashboard
        xaf_login(page, "User")
        titles = get_dashboard_titles(page)
        assert "User Dashboard" in titles, "Precondition: User should see User Dashboard"
        xaf_logout(page)

        # Admin: find and delete the Default-User Dashboard assignment
        xaf_login(page, "Admin")
        open_role_assignment_list(page)

        # Click on the assignment for "User Dashboard - Default"
        page.locator(
            '.xaf-listview [class*="GridRow"]:has-text("User Dashboard"):has-text("Default")'
        ).first.click()
        page.wait_for_selector(".xaf-detailview", timeout=15_000)
        page.click('[data-action-id="Delete"]')
        page.wait_for_timeout(500)
        confirm_btn = page.locator('button:has-text("Yes"), button:has-text("OK")')
        if confirm_btn.is_visible():
            confirm_btn.click()
        page.wait_for_timeout(1000)

        # Now log in as User — should NOT see User Dashboard
        xaf_logout(page)
        xaf_login(page, "User")
        titles = get_dashboard_titles(page)
        assert "User Dashboard" not in titles, (
            f"User should no longer see User Dashboard. Visible: {titles}"
        )

        # Cleanup: restore the assignment
        xaf_logout(page)
        xaf_login(page, "Admin")
        open_role_assignment_list(page)
        create_role_assignment(page, "User Dashboard", "Default")
