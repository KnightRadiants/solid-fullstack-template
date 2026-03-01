resource "aws_organizations_account" "app_safe" {
  for_each = local.is_debug_mode ? {} : local.accounts

  name              = each.value.name
  email             = each.value.email
  parent_id         = aws_organizations_organizational_unit.app.id
  role_name         = var.account_role_name
  close_on_deletion = false
  tags              = each.value.tags

  lifecycle {
    prevent_destroy = true
  }
}

resource "aws_organizations_account" "app_debug" {
  for_each = local.is_debug_mode ? local.accounts : {}

  name              = each.value.name
  email             = each.value.email
  parent_id         = aws_organizations_organizational_unit.app.id
  role_name         = var.account_role_name
  close_on_deletion = true
  tags              = each.value.tags
}
