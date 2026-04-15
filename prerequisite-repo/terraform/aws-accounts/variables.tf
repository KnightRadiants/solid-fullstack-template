variable "aws_region" {
  description = "AWS region used by the provider."
  type        = string
  default     = "eu-central-1"
}

variable "app_slug" {
  description = "Application slug used to derive the organization unit name."
  type        = string

  validation {
    condition     = can(regex("^[a-z0-9-]+$", var.app_slug))
    error_message = "app_slug must use lowercase letters, numbers, and hyphens only."
  }
}

variable "tags" {
  description = "Additional tags added to the organization unit."
  type        = map(string)
  default     = {}
}

variable "root_email_base" {
  description = "Base mailbox used to generate unique account root emails (without plus alias)."
  type        = string

  validation {
    condition     = can(regex("^[^+@\\s]+@[^@\\s]+\\.[^@\\s]+$", var.root_email_base))
    error_message = "root_email_base must be a valid email without '+' alias, e.g. mateusz@outlook.com."
  }
}

variable "environment_accounts" {
  description = "Environment account types to create inside the application OU."
  type        = list(string)
  default     = ["logging", "prod", "dev", "preview"]

  validation {
    condition     = length(var.environment_accounts) > 0
    error_message = "environment_accounts must include at least one environment account."
  }

  validation {
    condition     = alltrue([for key in var.environment_accounts : can(regex("^[a-z0-9-]+$", lower(key)))])
    error_message = "Each environment account name must contain only letters, numbers, and hyphens."
  }

  validation {
    condition     = length(distinct([for key in var.environment_accounts : lower(key)])) == length(var.environment_accounts)
    error_message = "environment_accounts must be unique (case-insensitive)."
  }
}

variable "account_role_name" {
  description = "Role name automatically created in each new member account."
  type        = string
  default     = "OrganizationAccountAccessRole"
}

variable "debug_suffix" {
  description = "Optional suffix used in debug runs to differentiate account names/emails across repeated test cycles."
  type        = string
  default     = ""

  validation {
    condition     = can(regex("^[a-z0-9-]*$", var.debug_suffix))
    error_message = "debug_suffix may contain only lowercase letters, numbers, and hyphens."
  }
}

variable "bootstrap_mode" {
  description = "safe keeps prevent_destroy on accounts; debug allows account closing on destroy."
  type        = string
  default     = "safe"

  validation {
    condition     = contains(["safe", "debug"], var.bootstrap_mode)
    error_message = "bootstrap_mode must be either 'safe' or 'debug'."
  }
}

