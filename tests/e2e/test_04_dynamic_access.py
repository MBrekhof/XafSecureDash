"""Dynamic access change tests."""
import pytest
from playwright.sync_api import Page
import sys
sys.path.insert(0, "..")
from conftest import (
    xaf_login, xaf_logout, navigate_to_role_assignments,
    get_dashboard_titles, BASE_URL, log,
)


def fill_lookup(page: Page, label_text: str, value: str):
    """Fill a XAF combobox lookup field by typing to trigger autocomplete, then selecting."""
    log.info("Filling lookup '%s' = '%s'", label_text, value)
    item = page.locator(f'dxbl-form-layout-item:has(label:text-is("{label_text}"))').first
    combo_input = item.locator("input[role='combobox']").first
    combo_input.click()
    page.wait_for_timeout(300)
    combo_input.fill("")
    combo_input.press_sequentially(value, delay=80)
    page.wait_for_timeout(2000)
    # Wait for the dropdown listbox to appear and select the matching option
    option = page.locator(f"li[role='option']:has-text(\"{value}\")").first
    option.wait_for(timeout=10_000)
    option.click()
    page.wait_for_timeout(1000)


def create_role_assignment(page: Page, dashboard_title: str, role_name: str):
    """Create a new DashboardRoleAssignment via XAF detail view."""
    log.info("Creating assignment: %s -> %s", dashboard_title, role_name)
    navigate_to_role_assignments(page)
    page.locator('button:has-text("New")').first.click()
    page.wait_for_timeout(3000)

    fill_lookup(page, "Dashboard", dashboard_title)
    fill_lookup(page, "Role", role_name)

    page.click('button[data-action-name="Save"]')
    page.wait_for_timeout(3000)
    log.info("Assignment created")


def delete_assignment_by_text(page: Page, text: str):
    """Delete a role assignment row containing the given text."""
    log.info("Deleting assignment containing '%s'", text)
    navigate_to_role_assignments(page)
    row = page.locator(f'.dxbl-grid tr.cursor-pointer:has-text("{text}")').first
    row.click()
    page.wait_for_timeout(3000)
    page.click('button[data-action-name="Delete"]')
    page.wait_for_timeout(2000)
    # XAF confirmation dialog uses data-action-name="Yes"
    yes_btn = page.locator('button[data-action-name="Yes"]').first
    yes_btn.wait_for(state="visible", timeout=10_000)
    yes_btn.click()
    page.wait_for_timeout(3000)
    log.info("Assignment deleted")


@pytest.mark.dynamic
class TestDynamicAccessChanges:
    def test_grant_manager_access_to_user_dashboard(self, page: Page):
        """Admin grants Manager access to User Dashboard; Manager then sees it."""
        xaf_login(page, "Admin")
        create_role_assignment(page, "User Dashboard", "Manager")

        xaf_logout(page)
        xaf_login(page, "Manager")
        titles = get_dashboard_titles(page)
        assert "User Dashboard" in titles, f"Manager should see User Dashboard. Got: {titles}"

        # Cleanup
        xaf_logout(page)
        xaf_login(page, "Admin")
        delete_assignment_by_text(page, "User Dashboard - Manager")

    def test_revoke_user_access_to_user_dashboard(self, page: Page):
        """
        Revoking the Default role from User Dashboard while keeping a Manager
        assignment means User (Default role) can no longer see it, but Manager can.
        Note: removing ALL assignments makes a dashboard unrestricted (visible to all).
        """
        xaf_login(page, "Admin")
        # First add a Manager assignment so we can safely remove Default
        create_role_assignment(page, "User Dashboard", "Manager")

        # Now delete the Default assignment
        delete_assignment_by_text(page, "User Dashboard - Default")

        # User (Default role) should no longer see it
        xaf_logout(page)
        xaf_login(page, "User")
        titles = get_dashboard_titles(page)
        assert "User Dashboard" not in titles, f"User should not see it. Got: {titles}"

        # Manager should still see it (has Manager assignment)
        xaf_logout(page)
        xaf_login(page, "Manager")
        titles = get_dashboard_titles(page)
        assert "User Dashboard" in titles, f"Manager should see it. Got: {titles}"

        # Restore: re-add Default assignment and remove Manager assignment
        xaf_logout(page)
        xaf_login(page, "Admin")
        create_role_assignment(page, "User Dashboard", "Default")
        delete_assignment_by_text(page, "User Dashboard - Manager")

    def test_removing_all_assignments_makes_dashboard_public(self, page: Page):
        """When all role assignments are removed, dashboard becomes unrestricted."""
        xaf_login(page, "Admin")
        delete_assignment_by_text(page, "User Dashboard - Default")

        # With no assignments, User Dashboard should be visible to everyone
        xaf_logout(page)
        xaf_login(page, "User")
        titles = get_dashboard_titles(page)
        assert "User Dashboard" in titles, f"User should see unrestricted dashboard. Got: {titles}"

        xaf_logout(page)
        xaf_login(page, "Manager")
        titles = get_dashboard_titles(page)
        assert "User Dashboard" in titles, f"Manager should see unrestricted dashboard. Got: {titles}"

        # Restore
        xaf_logout(page)
        xaf_login(page, "Admin")
        create_role_assignment(page, "User Dashboard", "Default")
