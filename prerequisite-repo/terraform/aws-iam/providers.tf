provider "aws" {
  region = var.aws_region

  assume_role {
    role_arn = local.account_access_role_arn
  }
}
