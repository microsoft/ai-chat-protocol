#!/bin/bash

find extra/secrets -type f -exec git update-index --skip-worktree {} ';'
