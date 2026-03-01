output "ou_id" {
  value = aws_organizations_organizational_unit.app.id
}

output "ou_arn" {
  value = aws_organizations_organizational_unit.app.arn
}

output "ou_name" {
  value = aws_organizations_organizational_unit.app.name
}
